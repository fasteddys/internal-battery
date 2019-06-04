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
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberService : ISubscriberService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private IB2CGraph _graphClient { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }

        public SubscriberService(UpDiddyDbContext context, IConfiguration configuration, ICloudStorage cloudStorage, IB2CGraph graphClient, IRepositoryWrapper repository, ILogger<SubscriberService> logger)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _graphClient = graphClient;
            _repository = repository;
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
                        BackgroundJob.Enqueue<ScheduledJobs>(j =>  j.ImportSubscriberProfileDataAsync(subscriber, resume));

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
                // subscriber.SubscriberFile.Remove(oldFile);
                oldFile.IsDeleted = 1;

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
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync( subscriber, resume));

            return true;
        }

        public async Task<Dictionary<Guid, Guid>> GetSubscriberJobPostingFavoritesByJobGuid(Guid subscriberGuid, List<Guid> jobGuids)
        {
            var subscribers = await _repository.Subscriber.GetAllSubscribersAsync();
            var jobPostingFavorites = await _repository.JobPostingFavorite.GetAllJobPostingFavoritesAsync();
            var jobPostings = await _repository.JobPosting.GetAllJobPostings();
            jobPostings = jobPostings.Where(jp => jobGuids.Contains(jp.JobPostingGuid));

            var query = from jp in jobPostings
            join favorites in jobPostingFavorites on jp.JobPostingId equals favorites.JobPostingId
            join sub in subscribers on favorites.SubscriberId equals sub.SubscriberId
            where (sub.SubscriberGuid == subscriberGuid && favorites.IsDeleted == 0)
            select new {
                jobPostingGuid = jp.JobPostingGuid,
                jobPostingFavoriteGuid = favorites.JobPostingFavoriteGuid
            };
            var map = query.ToDictionary(x => x.jobPostingGuid, x => x.jobPostingFavoriteGuid);
            return map;
        }

        #region resume parsing 

        public async Task<bool> ImportResume(ResumeParse resumeParse, string resume)
        {
            try
            {
                // Get the subscriber 
                //                Subscriber subscriber = SubscriberFactory.GetSubscriberById(_db, resumeParse.SubscriberId);
                Subscriber subscriber = await _repository.Subscriber.GetSubscriberByIdAsync(resumeParse.SubscriberId);
                if (subscriber == null)
                {                   
                    return false;
                }

                // Import Contact Info 
                _ImportResumeContactInfo(subscriber, resumeParse, resume);
                // Import skills 
            //    _ImportSovrenSkills(db, subscriber, resume, syslog);
                // Import work history  
             //   _ImportSovrenWorkHistory(db, subscriber, resume, syslog);
                // Import education history  
             //   _ImportSovrenEducationHistory(db, subscriber, resume, syslog);

                return true;
            }
            catch (Exception ex)
            {
             
                return false;
            }
        }





        private  async Task<bool> _ImportResumeContactInfo(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            try
            {
                // TODO JAB  implement 

                SubscriberContactInfoDto contactInfo = Utils.ParseContactInfoFromHrXML(resume);
                // case of existing property is empty or existing property and parsed property are equal 
                if ( string.IsNullOrEmpty(subscriber.FirstName) || subscriber.FirstName.Trim() == string.Empty  || subscriber.FirstName.Trim() == contactInfo.FirstName.Trim())
                {
                    subscriber.FirstName = contactInfo.FirstName.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, "First Name", "Subscriber", "FirstName", subscriber.FirstName, contactInfo.FirstName, (int) ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, "First Name", "Subscriber", "FirstName", subscriber.FirstName, contactInfo.FirstName, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);

 
                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrEmpty(subscriber.LastName) || subscriber.LastName.Trim() == string.Empty || subscriber.LastName.Trim() == contactInfo.LastName.Trim())
                {
                    subscriber.LastName = contactInfo.LastName.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, "Last Name", "Subscriber", "LastName", subscriber.LastName, contactInfo.LastName, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, "Last Name", "Subscriber", "LastName", subscriber.LastName, contactInfo.LastName, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);


                //TODO JAB remove test 
                subscriber.FirstName = "Maia";
                // save the subscriber
                _repository.Subscriber.SaveAsync();
                // save resume parse results 
                _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();





                /*
                subscriber.FirstName = contactInfo.FirstName;
                subscriber.LastName = contactInfo.LastName;
                contactInfo.PhoneNumber = contactInfo.PhoneNumber.Trim().Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                Regex phoneRegex = new Regex(@"^([0-9]{0,3})?[2-9]{1}[0-9]{9}$");
                if (phoneRegex.IsMatch(contactInfo.PhoneNumber))
                    subscriber.PhoneNumber = Utils.RemoveNonNumericCharacters(contactInfo.PhoneNumber);
                subscriber.City = contactInfo.City;
                subscriber.Address = contactInfo.Address;
                subscriber.PostalCode = contactInfo.PostalCode;
                State state = StateFactory.GetStateByStateCode(db, contactInfo.State);
                if (state != null)
                    subscriber.StateId = state.StateId;
                */







                ////_AddSubscriberContactInfo(db, subscriber, contactInfo);
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Subscriber:_ImportSovrenSkills threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }
        }


        #endregion



    }



}