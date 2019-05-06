using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberService : ISubscriberService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private IB2CGraph _graphClient { get; set; }
        private ILogger _logger { get; set; }

        public SubscriberService(UpDiddyDbContext context, IConfiguration configuration, ICloudStorage cloudStorage, IB2CGraph graphClient, ILogger<SubscriberService> logger)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task<SubscriberFile> AddResumeAsync(Subscriber subscriber, string fileName, Stream fileStream, bool parseResume = false)
        {

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    SubscriberFile resume = await _AddResumeAsync(subscriber, fileName, fileStream);
                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    if (parseResume)
                        BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync(resume));

                    return resume;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

        }

        public async Task<Subscriber> CreateSubscriberAsync(Guid partnerContactGuid, SignUpDto signUpDto)
        {
            var partnerContact = await _db.PartnerContact
                .Include(pc => pc.Contact)
                .Where(pc => pc.PartnerContactGuid == partnerContactGuid && pc.IsDeleted == 0 && pc.Contact.IsDeleted == 0)
                .FirstOrDefaultAsync();

            Campaign campaign = await _db.Campaign
                .Where(camp => camp.CampaignGuid.Equals(signUpDto.campaignGuid)
                    && camp.IsDeleted == 0
                    && camp.StartDate <= DateTime.UtcNow
                    && (!camp.EndDate.HasValue || camp.EndDate.Value >= DateTime.UtcNow))
                .FirstOrDefaultAsync();

            #region Verify and Check Data
            if (partnerContact == null)
                throw new ArgumentException("Invalid PartnerContact guid.");

            if (campaign == null)
                throw new ArgumentException("Signing up through this campaign is not available at this time.");

            // check email
            if (partnerContact.Contact.Email != signUpDto.email)
                throw new ArgumentException("This offer is only good for the recepient of this email campaign.");

            // check if subscriber is in database
            Subscriber subscriber = await _db.Subscriber.Where(s => s.Email == partnerContact.Contact.Email).FirstOrDefaultAsync();
            if (subscriber != null)
                throw new ArgumentException("Subscriber already exists, please login to continue.");
            #endregion

            User b2cUser = await _CreateB2CUser(signUpDto.email, signUpDto.password);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    Guid subscriberGuid = Guid.Parse(b2cUser.AdditionalData["objectId"].ToString());
                    Subscriber newSubscriber = new Subscriber()
                    {
                        SubscriberGuid = subscriberGuid,
                        ModifyGuid = subscriberGuid,
                        CreateGuid = subscriberGuid,
                        CreateDate = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow,
                        IsDeleted = 0,
                        IsVerified = true,
                        FirstName = partnerContact.Metadata["FirstName"].ToString(),
                        LastName = partnerContact.Metadata["LastName"].ToString(),
                        Email = partnerContact.Contact.Email,
                        PhoneNumber = partnerContact.Metadata["MobilePhone"].ToString(),
                        Address = string.Join(' ', partnerContact.Metadata["Address1"].ToString(), partnerContact.Metadata["Address2"].ToString()),
                        City = partnerContact.Metadata["City"].ToString(),
                        PostalCode = partnerContact.Metadata["PostalCode"].ToString(),
                        State = _db.State.Where(s => s.Name == partnerContact.Metadata["State"].ToString()).FirstOrDefault()
                    };
                    _db.Subscriber.Add(newSubscriber);
                    await _db.SaveChangesAsync();

                    partnerContact.Contact.SubscriberId = newSubscriber.SubscriberId;
                    CampaignPhase campaignPhase = CampaignPhaseFactory.GetCampaignPhaseByNameOrInitial(_db, campaign.CampaignId, signUpDto.campaignPhase);
                    _db.PartnerContactAction.Add(new PartnerContactAction()
                    {
                        ActionId = 3, // todo: use constants or enum or something
                        CampaignId = campaign.CampaignId,
                        PartnerContactId = partnerContact.PartnerContactId,
                        PartnerContactActionGuid = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        ModifyDate = DateTime.UtcNow,
                        ModifyGuid = Guid.Empty,
                        OccurredDate = DateTime.UtcNow,
                        CampaignPhaseId = campaignPhase.CampaignPhaseId
                    });

                    var file = await _db.PartnerContactFile.Where(e => e.PartnerContactId == partnerContact.PartnerContactId)
                        .OrderByDescending(e => e.CreateDate)
                        .FirstOrDefaultAsync();

                    if (file != null)
                    {
                        var bytes = Convert.FromBase64String(file.Base64EncodedData);
                        var contents = new MemoryStream(bytes);
                        await _AddResumeAsync(newSubscriber, file.Name, contents);
                    }

                    // associate the contact with the subscriber
                    var contact = _db.Contact.Where(c => c.ContactId == partnerContact.ContactId).FirstOrDefault();
                    contact.SubscriberId = partnerContact.Contact.SubscriberId;

                    // assign subscriber source for campaigns
                    SubscriberProfileStagingStore attribution = new SubscriberProfileStagingStore()
                    {
                        CreateDate = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow,
                        ModifyGuid = Guid.Empty,
                        CreateGuid = Guid.Empty,
                        SubscriberId = partnerContact.Contact.SubscriberId.Value,
                        ProfileSource = UpDiddyLib.Helpers.Constants.DataSource.CareerCircle,
                        IsDeleted = 0,
                        ProfileFormat = UpDiddyLib.Helpers.Constants.DataFormat.Json,
                        ProfileData = JsonConvert.SerializeObject(new { source = "campaign-sign-up", referer = campaign.Name })
                    };
                    _db.SubscriberProfileStagingStore.Add(attribution);

                    await _db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }

                return subscriber;
            }
        }

        /// <summary>
        /// Creates B2C User if one doesn't exist already.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<User> _CreateB2CUser(string email, string password)
        {
            Microsoft.Graph.User user = await _graphClient.GetUserBySignInEmail(email);
            if (user == null)
                user = await _graphClient.CreateUser(email, email, password);

            return user;
        }

        /// <summary>
        /// Uploads file to cloud storage and associates the file with Subscriber Entity.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private async Task<SubscriberFile> _AddResumeAsync(Subscriber subscriber, string fileName, Stream fileStream)
        {
            string blobName = await _cloudStorage.UploadFileAsync(String.Format("{0}/{1}/", subscriber.SubscriberGuid, "resume"), fileName, fileStream);
            SubscriberFile subscriberFileResume = new SubscriberFile
            {
                BlobName = blobName,
                ModifyGuid = subscriber.SubscriberGuid.Value,
                CreateGuid = subscriber.SubscriberGuid.Value,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                SubscriberId = subscriber.SubscriberId
            };

            // check to see if file is already in the system, if there is a file in the system in already then delete it
            // todo: refactor as part of multiple file upload/management system
            // todo: move logic to OnDelete event or somewhere centralized and run as a transaction somehow
            if (subscriber.SubscriberFile.Count > 0)
            {
                SubscriberFile oldFile = subscriber.SubscriberFile.Last();
                await _cloudStorage.DeleteFileAsync(oldFile.BlobName);
                subscriber.SubscriberFile.Remove(oldFile);
            }

            subscriber.SubscriberFile.Add(subscriberFileResume);

            return subscriberFileResume;
        }

        public async Task<Stream> GetResumeAsync(Subscriber subscriber)
        {
            SubscriberFile file = subscriber.SubscriberFile.Where(
                f => f.SubscriberFileGuid.Equals(
                    subscriber.SubscriberFile.FirstOrDefault()?.SubscriberFileGuid)).First();

            if (file == null)
                return null;
            
            return await _cloudStorage.OpenReadAsync(file.BlobName);
        }

        public async Task<bool> QueueScanResumeJobAsync(Guid subscriberGuid)
        {
            Subscriber subscriber = await _db.Subscriber
                .Where(e => e.IsDeleted == 0 && e.SubscriberGuid == subscriberGuid)
                .Include(e => e.SubscriberFile)
                .FirstOrDefaultAsync();

            SubscriberFile resume = subscriber.SubscriberFile.OrderByDescending(e => e.CreateDate).FirstOrDefault();

            if(resume != null)
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync(resume));

            return true;
        }
    }
}
