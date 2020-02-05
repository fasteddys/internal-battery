using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using UpDiddyLib.Dto.User;
using UpDiddyLib.Shared;
using UpDiddyLib.Helpers;
using System.Web;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Helpers;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberService : ISubscriberService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;
        private ITaggingService _taggingService { get; set; }
        private IHangfireService _hangfireService { get; set; }
        private IFileDownloadTrackerService _fileDownloadTrackerService { get; set; }
        private ISysEmail _sysEmail;
        private readonly IButterCMSService _butterCMSService;
        private readonly ZeroBounceApi _zeroBounceApi;


        public SubscriberService(UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            ITaggingService taggingService,
            IHangfireService hangfireService,
            IFileDownloadTrackerService fileDownloadTrackerService,
            ISysEmail sysEmail,
            IButterCMSService butterCMSService)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _taggingService = taggingService;
            _hangfireService = hangfireService;
            _fileDownloadTrackerService = fileDownloadTrackerService;
            _sysEmail = sysEmail;
            _zeroBounceApi = new ZeroBounceApi(_configuration, _repository, _logger);
            _butterCMSService = butterCMSService;
            
        }

        public async Task<Subscriber> GetSubscriberByEmail(string email)
        {
            return _repository.SubscriberRepository.GetSubscriberByEmail(email);
        }

        public async Task<Subscriber> GetSubscriberByGuid(Guid subscriberGuid)
        {
            return await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
        }


        public async Task<IList<SubscriberSourceDto>> GetSubscriberSources(int subscriberId)
        {
            return await _repository.StoredProcedureRepository.GetSubscriberSources(subscriberId);

        }

        public async Task<List<Subscriber>> GetSubscribersToIndexIntoGoogle(int numSubscribers, int indexVersion)
        {
            var querableSubscribers = _repository.SubscriberRepository.GetAllSubscribersAsync();

            List<Subscriber> rVal = await querableSubscribers.Where(s => s.IsDeleted == 0 && (s.CloudTalentIndexVersion < indexVersion || s.CloudTalentIndexVersion == 0))
                                                            .Take(numSubscribers)
                                                            .ToListAsync();

            if (rVal.Count > 0)
            {
                // build sql to update subscribers who will be updated 
                string updateSql = $"update subscriber set CloudTalentIndexVersion = {indexVersion} where subscriberid in (";
                string inList = string.Empty;
                foreach (Subscriber s in rVal)
                {
                    if (string.IsNullOrEmpty(inList) == false)
                        inList += ",";
                    inList += s.SubscriberId.ToString();

                }
                updateSql += inList + ")";
                await _repository.SubscriberRepository.ExecuteSQL(updateSql);

            }

            return rVal;
        }

        public async Task<bool> ToggleSubscriberNotificationEmail(Guid subscriberGuid, bool isNotificationEmailsEnabled)
        {
            bool isOperationSuccessful = false;
            try
            {
                var subscriber = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
                if (subscriber == null)
                    throw new ApplicationException($"Unrecognized subscriber; subscriberGuid: {subscriberGuid}");
                subscriber.NotificationEmailsEnabled = isNotificationEmailsEnabled;
                subscriber.ModifyDate = DateTime.UtcNow;
                subscriber.ModifyGuid = Guid.Empty;
                _repository.SubscriberRepository.Update(subscriber);
                await _repository.SubscriberRepository.SaveAsync();
                isOperationSuccessful = true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.ToggleSubscriberNotificationEmail: An error occured while attempting to modify the subscriber. Message: {e.Message}", e);
            }
            return isOperationSuccessful;
        }

        public async Task<SubscriberFile> AddResumeAsync(Subscriber subscriber, IFormFile resumeDoc, bool parseResume = false)
        {

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    SubscriberFile resume = await _AddResumeAsync(subscriber, resumeDoc.FileName, resumeDoc.OpenReadStream(), resumeDoc.ContentType);
                    await _repository.SubscriberFileRepository.Create(resume);
                    await _repository.SaveAsync();
                    transaction.Commit();

                    if (parseResume)
                        _hangfireService.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync(subscriber, resume));

                    return resume;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

        }


        public async Task<bool> CreateNewSubscriberAsync(SubscribeProfileBasicDto subscribeProfileBasicDto)
        {
            bool isSubscriberCreatedSuccessfully = false;

            try
            {

                var Subscriber = await GetSubscriberByGuid(subscribeProfileBasicDto.SubscriberGuid);
                if (Subscriber != null)
                    throw new AlreadyExistsException($"SubscriberGuid {subscribeProfileBasicDto.SubscriberGuid} already exists");

                Subscriber = await GetSubscriberByEmail(subscribeProfileBasicDto.Email);
                if (Subscriber != null)
                    throw new AlreadyExistsException($"A subscriber already exists with {subscribeProfileBasicDto.Email} as their email");



                int? StateId = null;
                if (string.IsNullOrWhiteSpace(subscribeProfileBasicDto.ProvinceCode) == false)
                {
                    State state = await StateFactory.GetStateByStateCode(_repository, subscribeProfileBasicDto.ProvinceCode);
                    if (state != null)
                        StateId = state.StateId;
                }

                // create the user in the CareerCircle database
                await _repository.SubscriberRepository.Create(new Subscriber()
                {
                    SubscriberGuid = subscribeProfileBasicDto.SubscriberGuid,
                    Auth0UserId = subscribeProfileBasicDto.Auth0UserId,
                    Email = subscribeProfileBasicDto.Email,
                    FirstName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.FirstName) ? subscribeProfileBasicDto.FirstName : null,
                    LastName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.LastName) ? subscribeProfileBasicDto.LastName : null,
                    PhoneNumber = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PhoneNumber) ? subscribeProfileBasicDto.PhoneNumber : null,
                    Address = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.Address) ? subscribeProfileBasicDto.Address : null,
                    City = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.City) ? subscribeProfileBasicDto.City : null,
                    PostalCode = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PostalCode) ? subscribeProfileBasicDto.PostalCode : null,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    StateId = StateId,
                    IsDeleted = 0
                });



                await _repository.SubscriberRepository.SaveAsync();
                var subscriber = _repository.SubscriberRepository.GetSubscriberByGuid(subscribeProfileBasicDto.SubscriberGuid);

                // add the user to the Google Talent Cloud
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscribeProfileBasicDto.SubscriberGuid));


                isSubscriberCreatedSuccessfully = true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.CreateNewSubscriberAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw (e);
            }

            return isSubscriberCreatedSuccessfully;

        }

        /// <summary>
        /// This is the new service that should be used by the Auth0 pre user registration hook's controller endpoint 
        /// </summary>
        /// <param name="subscriberDto"></param>
        /// <returns></returns>
        public async Task<Guid> CreateSubscriberAsync(UpDiddyLib.Domain.Models.SubscriberDto subscriberDto)
        {


            _logger.LogInformation($"SubscriberService:CreateSubscriberAsync  Creating account for {subscriberDto?.Email}");

            Models.Group group = null;
            var subscriberGuid = Guid.NewGuid();

            // create the user in the CareerCircle database
            await _repository.SubscriberRepository.Create(new Subscriber()
            {
                SubscriberGuid = subscriberGuid,
                Email = subscriberDto.Email,
                FirstName = !string.IsNullOrWhiteSpace(subscriberDto.FirstName) ? subscriberDto.FirstName : null,
                LastName = !string.IsNullOrWhiteSpace(subscriberDto.LastName) ? subscriberDto.LastName : null,
                PhoneNumber = !string.IsNullOrWhiteSpace(subscriberDto.PhoneNumber) ? subscriberDto.PhoneNumber : null,
                Auth0UserId = subscriberDto.Auth0UserId,
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.Empty,
                IsDeleted = 0
            });
            await _repository.SubscriberRepository.SaveAsync();

            // todo: if we remove the dependency for INT PKs from other services (tagging, file download) then we can remove the following line of code
            var subscriber = _repository.SubscriberRepository.GetSubscriberByGuid(subscriberGuid);

            // add the user to the Google Talent Cloud
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriberGuid));

            if (!string.IsNullOrWhiteSpace(subscriberDto.ReferrerUrl) && subscriberDto.PartnerGuid != null && subscriberDto.PartnerGuid != Guid.Empty)
            {
                _logger.LogInformation($"SubscriberService:CreateSubscriberAsync  Creating Group for URL {subscriberDto?.ReferrerUrl} and partner {subscriberDto.PartnerGuid}");
                // use the new tagging service for attribution
                group = await _taggingService.CreateGroup(subscriberDto.ReferrerUrl, subscriberDto.PartnerGuid, subscriber.SubscriberId);
            }
            else if (!string.IsNullOrWhiteSpace(subscriberDto.ReferrerUrl))
            {

                _logger.LogInformation($"SubscriberService:CreateSubscriberAsync looking up partnerguid from ReferrerUrl");
                Guid partnerGuid = Guid.Empty;
                bool isGatedDownload = false;
                string gatedDownloadFileUrl = string.Empty;
                decimal? maxFileDownloadAttemptsPermitted = null;
                int step = 0;
                // try catch the attempt to get the partner from butter 
                try
                {
                    var url = subscriberDto.ReferrerUrl;
                    step = 1;
                    var pageName = Path.GetFileName(url);
                    if (!string.IsNullOrEmpty(pageName))
                    {
                        if (pageName.Contains("?"))
                        {
                            pageName = pageName.Split("?")[0];
                        }
                    }
                    _logger.LogInformation($"SubscriberService:CreateSubscriberAsync RefererUrl = {subscriberDto.ReferrerUrl} PageName = {pageName}");
                    step = 2;
                    var butterPage = await _butterCMSService.RetrievePageAsync<CampaignLandingPage>(pageName);
                    if (butterPage != null)
                    {
                        isGatedDownload = butterPage.Data.Fields.isgateddownload;
                        gatedDownloadFileUrl = butterPage.Data.Fields.gatedfiledownloadfile;
                        maxFileDownloadAttemptsPermitted = butterPage.Data.Fields.gatedfiledownloadmaxattemptsallowed;
                        step = 3;
                        if(butterPage.Data.Fields.partner != null)
                        {
                            if(butterPage.Data.Fields.partner.PartnerGuid != Guid.Empty)
                            {
                                partnerGuid = butterPage.Data.Fields.partner.PartnerGuid;
                               _logger.LogInformation($"SubscriberService:CreateSubscriberAsync This campaign slug '{pageName}' does contain Partner with Guid {partnerGuid.ToString()}");
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"SubscriberService:CreateSubscriberAsync This campaign slug '{pageName}' does not have a partner associated with it");

                        }
                    }
                    else
                    {
                        _logger.LogInformation($"SubscriberService:CreateSubscriberAsync No campaign associated with slug '{pageName}'");

                    }
                    step = 5;
                    group = await _taggingService.CreateGroup(subscriberDto.ReferrerUrl, partnerGuid, subscriber.SubscriberId);

                    if (butterPage.Data.Fields.isgateddownload && !string.IsNullOrEmpty(gatedDownloadFileUrl))
                    {
                        int? groupId = null;
                        if (group != null)
                            groupId = group.GroupId;
                        int? maxAllowedInt = null;
                        if (maxFileDownloadAttemptsPermitted.HasValue)
                            maxAllowedInt = Decimal.ToInt16(maxFileDownloadAttemptsPermitted.Value);
                        await HandleGatedFileDownload(maxAllowedInt, gatedDownloadFileUrl, groupId, subscriber.SubscriberId, subscriber.Email);
                    }

                    if (!string.IsNullOrEmpty(subscriberDto.AssessmentId))
                    {
                        await TraitifyHelper.CompleteSignup(subscriberDto.AssessmentId, subscriber, _logger, _repository, _sysEmail, _configuration, _zeroBounceApi);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"SubscriberService:CreateSubscriberAsync Error at step {step} getting partner guid from url.  Msg = {ex.Message}");
                }
            }



            return subscriberGuid;
        }

        [Obsolete("This can be removed after we launch the new website (HelloBuild).", false)]
        public async Task<bool> CreateSubscriberAsync(CreateUserDto createUserDto)
        {
            bool isSubscriberCreatedSuccessfully = false;

            try
            {
                Models.Group group = null;
            
                // create the user in the CareerCircle database
                await _repository.SubscriberRepository.Create(new Subscriber()
                {
                    SubscriberGuid = createUserDto.SubscriberGuid,
                    Auth0UserId = createUserDto.Auth0UserId,
                    Email = createUserDto.Email,
                    FirstName = !string.IsNullOrWhiteSpace(createUserDto.FirstName) ? createUserDto.FirstName : null,
                    LastName = !string.IsNullOrWhiteSpace(createUserDto.LastName) ? createUserDto.LastName : null,
                    PhoneNumber = !string.IsNullOrWhiteSpace(createUserDto.PhoneNumber) ? createUserDto.PhoneNumber : null,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0
                });
                await _repository.SubscriberRepository.SaveAsync();
                var subscriber = _repository.SubscriberRepository.GetSubscriberByGuid(createUserDto.SubscriberGuid);

                // add the user to the Google Talent Cloud
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(createUserDto.SubscriberGuid));

                if (!string.IsNullOrWhiteSpace(createUserDto.ReferrerUrl) && createUserDto.PartnerGuid != null && createUserDto.PartnerGuid != Guid.Empty)
                {
                    // use the new tagging service for attribution
                    group = await _taggingService.CreateGroup(createUserDto.ReferrerUrl, createUserDto.PartnerGuid, subscriber.SubscriberId);
                }

                if (createUserDto.IsGatedDownload && !string.IsNullOrEmpty(createUserDto.GatedDownloadFileUrl))
                {
                    int? groupId = null;
                    if (group != null)
                        groupId = group.GroupId;
                    // set up the gated file download and send the email
                    await HandleGatedFileDownload(createUserDto.GatedDownloadMaxAttemptsAllowed, createUserDto.GatedDownloadFileUrl, groupId, subscriber.SubscriberId, subscriber.Email);
                }

                isSubscriberCreatedSuccessfully = true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.CreateSubscriberAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
            }

            return isSubscriberCreatedSuccessfully;
        }



        public async Task<bool> ExistingSubscriberSignUp(CreateUserDto createUserDto)
        {
            bool isGatedDownload = false;
            string gatedDownloadFileUrl = string.Empty;
            decimal? maxFileDownloadAttemptsPermitted = null;
            bool isSubscriberUpdatedSuccessfully = false;
            Guid partnerGuid = Guid.Empty;
            Models.Group group = null;
            try
            {
                Subscriber subscriber = await this.GetSubscriberByGuid(createUserDto.SubscriberGuid);
                if (subscriber == null)
                    throw new ApplicationException("Invalid subscriber identifier");

                if (!string.IsNullOrWhiteSpace(createUserDto.FirstName))
                    subscriber.FirstName = createUserDto.FirstName;
                if (!string.IsNullOrWhiteSpace(createUserDto.LastName))
                    subscriber.LastName = createUserDto.LastName;
                if (!string.IsNullOrWhiteSpace(createUserDto.PhoneNumber))
                    subscriber.PhoneNumber = createUserDto.PhoneNumber;
                await this.UpdateSubscriber(subscriber);

                _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));
                if (!string.IsNullOrWhiteSpace(createUserDto.ReferrerUrl))
                {
                    _logger.LogInformation($"SubscriberService:ExistingSubscriberSignUp looking up partnerguid from ReferrerUrl");
                    
                    var url = createUserDto.ReferrerUrl;
                    var pageName = Path.GetFileName(url);
                    if (!string.IsNullOrEmpty(pageName))
                    {
                        if (pageName.Contains("?"))
                        {
                            pageName = pageName.Split("?")[0];
                        }
                    }
                    _logger.LogInformation($"SubscriberService:ExistingSubscriberSignUp RefererUrl = {createUserDto.ReferrerUrl} PageName = {pageName}");
                    var butterPage = await _butterCMSService.RetrievePageAsync<CampaignLandingPage>(pageName);
                    if (butterPage != null)
                    {
                        isGatedDownload = butterPage.Data.Fields.isgateddownload;
                        gatedDownloadFileUrl = butterPage.Data.Fields.gatedfiledownloadfile;
                        maxFileDownloadAttemptsPermitted = butterPage.Data.Fields.gatedfiledownloadmaxattemptsallowed;
                       if(butterPage.Data.Fields.partner != null)
                        {
                            if(butterPage.Data.Fields.partner.PartnerGuid != Guid.Empty)
                            {
                                partnerGuid = butterPage.Data.Fields.partner.PartnerGuid;
                               _logger.LogInformation($"SubscriberService:CreateSubscriberAsync This campaign slug '{pageName}' does contain Partner with Guid {partnerGuid.ToString()}");
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"SubscriberService:CreateSubscriberAsync This campaign slug '{pageName}' does not have a partner associated with it");

                        }
                    }
                    else
                    {
                        _logger.LogInformation($"SubscriberService:ExistingSubscriberSignUp No campaign associated with slug '{pageName}'");
                    }

                    group = await _taggingService.CreateGroup(createUserDto.ReferrerUrl, partnerGuid, subscriber.SubscriberId);
                    if (isGatedDownload && !string.IsNullOrEmpty(gatedDownloadFileUrl))
                    {
                        int? groupId = null;
                        if (group != null)
                            groupId = group.GroupId;
                        int? maxAllowedInt = null;
                        if (maxFileDownloadAttemptsPermitted != null)
                            maxAllowedInt = Decimal.ToInt16(maxFileDownloadAttemptsPermitted.Value);
                        await HandleGatedFileDownload(maxAllowedInt, gatedDownloadFileUrl, groupId, subscriber.SubscriberId, subscriber.Email);
                    }


                    isSubscriberUpdatedSuccessfully = true;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.ExistingSubscriberSignUp: An error occured while attempting to update a subscriber. Message: {e.Message}", e);
            }

            return isSubscriberUpdatedSuccessfully;
        }

        private async Task HandleGatedFileDownload(int? maxDownloadAttempts, string fileUrl, int? groupId, int subscriberId, string email)
        {
            FileDownloadTrackerDto fileDownloadTrackerDto = new FileDownloadTrackerDto
            {
                SourceFileCDNUrl = fileUrl,
                MaxFileDownloadAttemptsPermitted = maxDownloadAttempts,
                SubscriberId = subscriberId,
                GroupId = groupId
            };
            string downloadUrl = await _fileDownloadTrackerService.CreateFileDownloadLink(fileDownloadTrackerDto);

            _hangfireService.Enqueue(() =>
             _sysEmail.SendTemplatedEmailAsync(
                 email,
                 _configuration["SysEmail:Transactional:TemplateIds:GatedDownload-LinkEmail"],
                 new
                 {
                     fileDownloadLinkUrl = downloadUrl
                 },
                 Constants.SendGridAccount.Transactional,
                 null,
                 null,
                 null,
                 null
             ));
        }



        public async Task<bool> UpdateSubscriberProfileBasicAsync(SubscribeProfileBasicDto subscribeProfileBasicDto, Guid subscriberGuid)
        {
            try
            {
                var Subscriber = await GetSubscriberByGuid(subscribeProfileBasicDto.SubscriberGuid);
                if (Subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscribeProfileBasicDto.SubscriberGuid} does not exist exist");

                if (Subscriber.Email != subscribeProfileBasicDto.Email)
                    throw new InvalidOperationException($"This operation cannot be used to change a subscriber's email address");


                if (Subscriber.SubscriberGuid != subscriberGuid)
                    throw new InvalidOperationException($"Not owner of profile");

                int? StateId = null;
                if (string.IsNullOrWhiteSpace(subscribeProfileBasicDto.ProvinceCode) == false)
                {
                    State state = await StateFactory.GetStateByStateCode(_repository, subscribeProfileBasicDto.ProvinceCode);
                    if (state != null)
                        StateId = state.StateId;
                }

                // update the user in the CareerCircle database
                Subscriber.SubscriberGuid = subscribeProfileBasicDto.SubscriberGuid;
                Subscriber.Auth0UserId = subscribeProfileBasicDto.Auth0UserId;
                Subscriber.FirstName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.FirstName) ? subscribeProfileBasicDto.FirstName : null;
                Subscriber.LastName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.LastName) ? subscribeProfileBasicDto.LastName : null;
                Subscriber.PhoneNumber = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PhoneNumber) ? subscribeProfileBasicDto.PhoneNumber : null;
                Subscriber.Address = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.Address) ? subscribeProfileBasicDto.Address : null;
                Subscriber.City = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.City) ? subscribeProfileBasicDto.City : null;
                Subscriber.PostalCode = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PostalCode) ? subscribeProfileBasicDto.PostalCode : null;
                Subscriber.ModifyDate = DateTime.UtcNow;
                Subscriber.StateId = StateId;

                await _repository.SubscriberRepository.SaveAsync();

                // add the user to the Google Talent Cloud
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscribeProfileBasicDto.SubscriberGuid));

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.UpdateSubscriberProfileBasicAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw e;
            }
            return true;
        }


        public async Task<SubscribeProfileBasicDto> GetSubscriberProfileBasicAsync(Guid subscriberGuid)
        {

            SubscribeProfileBasicDto rVal = null;
            try
            {
                var Subscriber = await GetSubscriberByGuid(subscriberGuid);
                if (Subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
                State state = await _repository.State.GetStateBySubscriberGuid(Subscriber.SubscriberGuid.Value);
                Subscriber.State = state;



                rVal = _mapper.Map<SubscribeProfileBasicDto>(Subscriber);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.UpdateSubscriberProfileBasicAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw e;
            }
            return rVal;
        }





        public async Task UpdateSubscriber(Subscriber subscriber)
        {
            subscriber.ModifyDate = DateTime.UtcNow;
            subscriber.ModifyGuid = Guid.Empty;
            _repository.SubscriberRepository.Update(subscriber);
            await _repository.SubscriberRepository.SaveAsync();
        }

        /// <summary>
        /// Uploads file to cloud storage and associates the file with Subscriber Entity.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private async Task<SubscriberFile> _AddResumeAsync(Subscriber subscriber, string fileName, Stream fileStream, string fileMimeType = null)
        {
            string blobName = await _cloudStorage.UploadFileAsync(String.Format("{0}/{1}/", subscriber.SubscriberGuid, "resume"), fileName, fileStream);
            SubscriberFile subscriberFileResume = new SubscriberFile
            {
                BlobName = blobName,
                ModifyGuid = subscriber.SubscriberGuid.Value,
                CreateGuid = subscriber.SubscriberGuid.Value,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                SubscriberId = subscriber.SubscriberId,
                MimeType = fileMimeType
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
            Subscriber subscriber = await _repository.SubscriberRepository.GetAllWithTracking()
                .Where(e => e.IsDeleted == 0 && e.SubscriberGuid == subscriberGuid)
                .Include(e => e.SubscriberFile)
                .FirstOrDefaultAsync();

            SubscriberFile resume = subscriber.SubscriberFile.OrderByDescending(e => e.CreateDate).FirstOrDefault();

            if (resume != null)
                _hangfireService.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync(subscriber, resume));

            return true;
        }

        public async Task<Dictionary<Guid, Guid>> GetSubscriberJobPostingFavoritesByJobGuid(Guid subscriberGuid, List<Guid> jobGuids)
        {
            var subscribers = _repository.SubscriberRepository.GetAllSubscribersAsync();
            var jobPostingFavorites = _repository.JobPostingFavorite.GetAllJobPostingFavoritesAsync();
            var jobPostings = _repository.JobPosting.GetAllJobPostings();
            jobPostings = jobPostings.Where(jp => jobGuids.Contains(jp.JobPostingGuid));

            var query = from jp in jobPostings
                        join favorites in jobPostingFavorites on jp.JobPostingId equals favorites.JobPostingId
                        join sub in subscribers on favorites.SubscriberId equals sub.SubscriberId
                        where (sub.SubscriberGuid == subscriberGuid && favorites.IsDeleted == 0)
                        select new
                        {
                            jobPostingGuid = jp.JobPostingGuid,
                            jobPostingFavoriteGuid = favorites.JobPostingFavoriteGuid
                        };
            var map = await query.ToDictionaryAsync(x => x.jobPostingGuid, x => x.jobPostingFavoriteGuid);
            return map;
        }

        public async Task<List<Subscriber>> GetFailedSubscribersSummaryAsync()
        {
            var query = _repository.SubscriberRepository.GetAll();
            return await query.Where(x => x.CloudTalentIndexStatus == 3 && x.IsDeleted == 0).ToListAsync();
        }

        public async Task<Subscriber> GetBySubscriberGuid(Guid subscriberGuid)
        {
            return await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
        }
        #region subscriber notes
        public async Task SaveSubscriberNotesAsync(SubscriberNotesDto subscriberNotesDto)
        {
            //check if notes exist for subscriber


            if (subscriberNotesDto.SubscriberNotesGuid == null)
            {
                var subscriberNotes = _mapper.Map<SubscriberNotes>(subscriberNotesDto);

                //Assign new GUID for new notes
                subscriberNotes.SubscriberNotesGuid = Guid.NewGuid();

                //get subscriber by subscriberGuid and assign SubscriberId
                var subscriber = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberNotesDto.SubscriberGuid);
                subscriberNotes.SubscriberId = subscriber.SubscriberId;

                //get subscriber by subscriberGuid  map to recruited and get recruiterId
                //recruiter is also a subscriber
                var recruiter = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberNotesDto.RecruiterGuid);
                var rec = await _repository.RecruiterRepository.GetRecruiterBySubscriberId(recruiter.SubscriberId);
                subscriberNotes.RecruiterId = rec.RecruiterId;

                BaseModelFactory.SetDefaultsForAddNew(subscriberNotes);
                await _repository.SubscriberNotesRepository.AddNotes(subscriberNotes);
            }
            else if (subscriberNotesDto.SubscriberNotesGuid != Guid.Empty)
            {
                var subscriberNote = await _repository.SubscriberNotesRepository.GetSubscriberNotesBySubscriberNotesGuid(subscriberNotesDto.SubscriberNotesGuid ?? Guid.Empty);
                subscriberNote.Notes = subscriberNotesDto.Notes;
                subscriberNote.ViewableByOthersInRecruiterCompany = subscriberNotesDto.ViewableByOthersInRecruiterCompany;
                subscriberNote.ModifyDate = DateTime.Now;
                await _repository.SubscriberNotesRepository.UpdateNotes(subscriberNote);
            }

        }

        public async Task<List<SubscriberNotesDto>> GetSubscriberNotesBySubscriberGuid(string subscriberGuid, string recruiterGuid, string searchquery)
        {
            List<SubscriberNotesDto> subscriberNotesDtoList = new List<SubscriberNotesDto>();
            //subscriberGuid
            Guid sGuid = Guid.Parse(subscriberGuid);
            Guid rGuid = Guid.Parse(recruiterGuid);

            //get subscriber record for candidate
            var subscriberData = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(sGuid);

            //get recruiter record
            //get recruiter by subscriberGuid  map to recruiter and get recruiterId
            //recruiter is also a subscriber
            var recruiterData = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(rGuid);
            var rec = await _repository.RecruiterRepository.GetRecruiterBySubscriberId(recruiterData.SubscriberId);

            List<SubscriberNotesDto> recruiterPrivateNotes;
            List<SubscriberNotesDto> subscriberPublicNotes;

            //get notes for subscriber that are private and visible to current logged in recruiter
            var recruiterPrivateNotesQueryable = from subscriberNote in _repository.SubscriberNotesRepository.GetAll()
                                                 join recruiter in _repository.RecruiterRepository.GetAll() on subscriberNote.RecruiterId equals recruiter.RecruiterId
                                                 join subscriber in _repository.SubscriberRepository.GetAll() on recruiter.SubscriberId equals subscriber.SubscriberId
                                                 where subscriberNote.SubscriberId.Equals(subscriberData.SubscriberId) && subscriberNote.IsDeleted.Equals(0) && subscriberNote.RecruiterId.Equals(rec.RecruiterId) && subscriberNote.ViewableByOthersInRecruiterCompany.Equals(false)
                                                 select new SubscriberNotesDto()
                                                 {
                                                     ViewableByOthersInRecruiterCompany = subscriberNote.ViewableByOthersInRecruiterCompany,
                                                     Notes = subscriberNote.Notes,
                                                     SubscriberNotesGuid = subscriberNote.SubscriberNotesGuid,
                                                     RecruiterGuid = (Guid)subscriber.SubscriberGuid,
                                                     SubscriberGuid = (Guid)subscriberData.SubscriberGuid,
                                                     CreateDate = subscriberNote.CreateDate,
                                                     ModifiedDate = (DateTime)subscriberNote.ModifyDate,
                                                     RecruiterName = recruiter.FirstName + " " + recruiter.LastName
                                                 };



            //get notes for subscriber that are public and visible to recruiters of current logged in recruiter company
            var subscriberPublicNotesQueryable = from subscriberNote in _repository.SubscriberNotesRepository.GetAll()
                                                 join recruiter in _repository.RecruiterRepository.GetAll() on subscriberNote.RecruiterId equals recruiter.RecruiterId
                                                 join company in _repository.Company.GetAllCompanies() on recruiter.CompanyId equals company.CompanyId
                                                 join subscriber in _repository.SubscriberRepository.GetAll() on recruiter.SubscriberId equals subscriber.SubscriberId
                                                 where subscriberNote.SubscriberId.Equals(subscriberData.SubscriberId) && subscriberNote.IsDeleted.Equals(0) && recruiter.CompanyId.Equals(rec.CompanyId) && subscriberNote.ViewableByOthersInRecruiterCompany.Equals(true)
                                                 select new SubscriberNotesDto()
                                                 {
                                                     ViewableByOthersInRecruiterCompany = subscriberNote.ViewableByOthersInRecruiterCompany,
                                                     Notes = subscriberNote.Notes,
                                                     SubscriberNotesGuid = subscriberNote.SubscriberNotesGuid,
                                                     RecruiterGuid = (Guid)subscriber.SubscriberGuid,
                                                     SubscriberGuid = (Guid)subscriberData.SubscriberGuid,
                                                     CreateDate = subscriberNote.CreateDate,
                                                     ModifiedDate = (DateTime)subscriberNote.ModifyDate,
                                                     RecruiterName = recruiter.FirstName + " " + recruiter.LastName
                                                 };

            if (!string.IsNullOrEmpty(searchquery))
            {
                recruiterPrivateNotes = await recruiterPrivateNotesQueryable.Where(pn => pn.CreateDate.Date == DateTime.Parse(searchquery).Date).ToListAsync();
                subscriberPublicNotes = await subscriberPublicNotesQueryable.Where(pn => pn.CreateDate.Date == DateTime.Parse(searchquery).Date).ToListAsync();
            }
            else
            {
                recruiterPrivateNotes = await recruiterPrivateNotesQueryable.ToListAsync();
                subscriberPublicNotes = await subscriberPublicNotesQueryable.ToListAsync();
            }


            subscriberNotesDtoList.AddRange(recruiterPrivateNotes);
            subscriberNotesDtoList.AddRange(subscriberPublicNotes);

            return subscriberNotesDtoList.OrderByDescending(sn => sn.CreateDate).ToList();
        }




        public async Task<bool> DeleteSubscriberNote(Guid subscriberNotesGuid)
        {
            bool isDeleted = false;
            //check if notes exist for subscriber
            var subscriberNote = await _repository.SubscriberNotesRepository.GetSubscriberNotesBySubscriberNotesGuid(subscriberNotesGuid);
            if (subscriberNote != null)
            {
                subscriberNote.IsDeleted = 1;
                await _repository.SubscriberNotesRepository.UpdateNotes(subscriberNote);
                isDeleted = true;
            }

            return isDeleted;
        }
        #endregion



        #region resume parsing 

        public async Task<bool> ImportResume(ResumeParse resumeParse, string resume)
        {
            try
            {
                bool requiresMerge = false;
                // Get the subscriber 
                //                Subscriber subscriber = SubscriberFactory.GetSubscriberById(_db, resumeParse.SubscriberId);
                Subscriber subscriber = await _repository.SubscriberRepository.GetSubscriberByIdAsync(resumeParse.SubscriberId);

                if (subscriber == null)
                {
                    return false;
                }
                // Import Contact Info 
                if (await _ImportResumeContactInfo(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;
                // Import skills 
                if (await _ImportResumeSkills(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;
                // Import work history  
                if (await _ImportResumeWorkHistory(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;
                // Import education history  
                if (await _ImportResumeEducationHistory(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;

                return requiresMerge;
            }
            catch (Exception)
            {

                return false;
            }
        }

        private async Task<bool> _ImportResumeEducationHistory(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            try
            {
                bool requiresMerge = false;
                List<SubscriberEducationHistoryDto> parsedEducationHistory = Utils.ParseEducationHistoryFromHrXml(resume);
                IList<SubscriberEducationHistory> educationHistory = await SubscriberFactory.GetSubscriberEducationHistoryById(_repository, subscriber.SubscriberId);
                foreach (SubscriberEducationHistoryDto eh in parsedEducationHistory)
                {
                    string parsedInstitutionName = eh.EducationalInstitution.ToLower();
                    string parsedEducationalDegree = eh.EducationalDegree.ToLower();
                    string parsedEducationalDegreeType = eh.EducationalDegreeType.ToLower();
                    var ExistingInstitution = educationHistory.Where(s => s.EducationalInstitution.Name.ToLower() == parsedInstitutionName).FirstOrDefault();
                    // get or create the company specified by the work history 
                    EducationalInstitution institution = await EducationalInstitutionFactory.GetOrAdd(_repository, parsedInstitutionName);
                    EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(_repository, parsedEducationalDegree);
                    // Do not allow user defined degree types so call GetOrDefault 
                    EducationalDegreeType educationalDegreeType = await EducationalDegreeTypeFactory.GetOrDefault(_repository, parsedEducationalDegreeType);
                    // if its not an existing college just add it

                    if (ExistingInstitution == null)
                    {

                        SubscriberEducationHistory newEducationHistory = await SubscriberEducationHistoryFactory.AddEducationHistoryForSubscriber(_repository, subscriber, eh, institution, educationalDegree, educationalDegreeType);
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "Object", string.Empty, string.Empty, (int)ResumeParseStatus.Merged, newEducationHistory.SubscriberEducationHistoryGuid);
                    }
                    else // Check to see its an update to an existing work history 
                    {
                        // todo - at some point make this more intelligent, edge cases as two degrees from the same institution may not be handled very well
                        // well
                        var ExistingInstitutions = educationHistory.Where(s => s.EducationalInstitution.Name.ToLower() == parsedInstitutionName).ToList();
                        foreach (SubscriberEducationHistory seh in ExistingInstitutions)
                            if (await MergeEducationalHistories(resumeParse, seh, eh) == true)
                                requiresMerge = true;
                    }

                }
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
                return requiresMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeWorkHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }

        }

        private async Task<bool> MergeEducationalHistories(ResumeParse resumeParse, SubscriberEducationHistory educationHistory, SubscriberEducationHistoryDto parsedEducationHistory)
        {
            bool requiresMerge = false;
            if (parsedEducationHistory.StartDate != null && parsedEducationHistory.StartDate != DateTime.MinValue)
            {
                if (educationHistory.StartDate == null || educationHistory.StartDate == DateTime.MinValue || educationHistory.StartDate == parsedEducationHistory.StartDate)
                {
                    educationHistory.StartDate = parsedEducationHistory.StartDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "StartDate", educationHistory.StartDate.Value.ToShortDateString(), parsedEducationHistory.StartDate.Value.ToShortDateString(), (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"When did you start studying at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory", "StartDate", educationHistory.StartDate.Value.ToShortDateString(), parsedEducationHistory.StartDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }


            if (parsedEducationHistory.EndDate != null && parsedEducationHistory.EndDate != DateTime.MinValue)
            {
                if (educationHistory.EndDate == null || educationHistory.EndDate == DateTime.MinValue || educationHistory.EndDate == parsedEducationHistory.EndDate)
                {
                    educationHistory.EndDate = parsedEducationHistory.EndDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "EndDate", educationHistory.EndDate.Value.ToShortDateString(), parsedEducationHistory.EndDate.Value.ToShortDateString(), (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"When did you complete your studies at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory", "EndDate", educationHistory.EndDate.Value.ToShortDateString(), parsedEducationHistory.EndDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }


            if (parsedEducationHistory.DegreeDate != null && parsedEducationHistory.DegreeDate != DateTime.MinValue)
            {
                if (educationHistory.DegreeDate == null || educationHistory.DegreeDate == DateTime.MinValue || educationHistory.DegreeDate == parsedEducationHistory.DegreeDate)
                {
                    educationHistory.DegreeDate = parsedEducationHistory.DegreeDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "DegreeDate", educationHistory.DegreeDate.Value.ToShortDateString(), parsedEducationHistory.DegreeDate.Value.ToShortDateString(), (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"When did you complete your degree at {educationHistory.EducationalInstitution.Name}?", "SubscriberWorkHistory", "DegreeDate", educationHistory.DegreeDate.Value.ToShortDateString(), parsedEducationHistory.DegreeDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }
            if (string.IsNullOrWhiteSpace(parsedEducationHistory.EducationalDegreeType) == false)
            {
                string degreeType = Utils.RemoveNewlines(educationHistory.EducationalDegreeType.DegreeType.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedDegreeType = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedEducationHistory.EducationalDegreeType.Trim()));

                if (string.IsNullOrWhiteSpace(degreeType) || degreeType.ToLower() == parsedDegreeType.ToLower())
                {
                    // case where current value is not specified but parsed value is 
                    if (string.IsNullOrWhiteSpace(degreeType) == true && string.IsNullOrWhiteSpace(parsedDegreeType) == false)
                    {
                        EducationalDegreeType newDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(_repository, parsedDegreeType);
                        educationHistory.EducationalDegreeTypeId = newDegreeType.EducationalDegreeTypeId;
                    }
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory.EducationalDegreeTypeId", "EducationalDegreeTypeId", degreeType, parsedDegreeType, (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    EducationalDegreeType educationalDegreeType = await EducationalDegreeTypeFactory.GetEducationalDegreeTypeByDegreeType(_repository, parsedDegreeType);
                    // only add educational degree type if the parsed value is in the CC list of degree types since
                    // degree types is a lookup list curated by CC and user's cannot add new value to it
                    if (educationalDegreeType != null)
                    {
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"What type of degree did you earn at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory.EducationalDegreeTypeId", "EducationalDegreeTypeId", degreeType, parsedDegreeType, (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                        requiresMerge = true;

                    }
                }

            }

            if (string.IsNullOrWhiteSpace(parsedEducationHistory.EducationalDegree) == false)
            {
                string degree = Utils.RemoveNewlines(educationHistory.EducationalDegree.Degree.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedDegree = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedEducationHistory.EducationalDegree.Trim()));

                if (string.IsNullOrWhiteSpace(degree) || degree.ToLower() == parsedDegree.ToLower())
                {
                    // case where current value is not specified but parsed value is 
                    if (string.IsNullOrWhiteSpace(degree) == true && string.IsNullOrWhiteSpace(parsedDegree) == false)
                    {
                        EducationalDegree newDegreeType = await EducationalDegreeFactory.GetOrAdd(_repository, parsedDegree);
                        educationHistory.EducationalDegreeId = newDegreeType.EducationalDegreeId;
                    }
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory.EducationalDegreeId", "EducationalDegreeId", degree, parsedDegree, (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"What was the major of your degree earned at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory.EducationalDegreeId", "EducationalDegreeId", degree, parsedDegree, (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }

            await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
            return requiresMerge;
        }






        /// <summary>
        ///  enhances and notes differences in user's work history on record vs their work history parsed from their resume
        /// </summary>
        /// <param name="resumeParse"></param>
        /// <param name="workHistory"></param>
        /// <param name="parsedWorkHistory"></param>
        /// <returns></returns>
        private async Task<bool> MergeWorkHistories(ResumeParse resumeParse, SubscriberWorkHistory existingWorkHistoryItem, SubscriberWorkHistoryDto parsedWorkHistory)
        {
            bool requireMerge = false;
            if (parsedWorkHistory.StartDate != null && parsedWorkHistory.StartDate != DateTime.MinValue)
            {
                if (existingWorkHistoryItem.StartDate == null || existingWorkHistoryItem.StartDate == DateTime.MinValue || existingWorkHistoryItem.StartDate == parsedWorkHistory.StartDate)
                {
                    existingWorkHistoryItem.StartDate = parsedWorkHistory.StartDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "StartDate", existingWorkHistoryItem.StartDate.Value.ToShortDateString(), parsedWorkHistory.StartDate.Value.ToString(), (int)ResumeParseStatus.Merged, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, $"When did you start employment at {existingWorkHistoryItem.Company.CompanyName}?", "SubscriberWorkHistory", "StartDate", existingWorkHistoryItem.StartDate.Value.ToShortDateString(), parsedWorkHistory.StartDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }

            if (parsedWorkHistory.EndDate != null && parsedWorkHistory.EndDate != DateTime.MinValue)
            {
                if (existingWorkHistoryItem.EndDate == null || existingWorkHistoryItem.EndDate == DateTime.MinValue || existingWorkHistoryItem.EndDate == parsedWorkHistory.EndDate)
                {
                    existingWorkHistoryItem.EndDate = parsedWorkHistory.EndDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "EndDate", existingWorkHistoryItem.EndDate.Value.ToShortDateString(), parsedWorkHistory.EndDate.Value.ToShortDateString(), (int)ResumeParseStatus.Merged, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, $"When did your employment at {existingWorkHistoryItem.Company.CompanyName} end?", "SubscriberWorkHistory", "EndDate", existingWorkHistoryItem.EndDate.Value.ToLongDateString(), parsedWorkHistory.EndDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }

            if (string.IsNullOrWhiteSpace(parsedWorkHistory.Title) == false)
            {
                string jobTitle = Utils.RemoveNewlines(existingWorkHistoryItem.Title.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedJobTitle = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedWorkHistory.Title.Trim()));


                if (string.IsNullOrWhiteSpace(jobTitle) || jobTitle.ToLower() == parsedJobTitle.ToLower())
                {
                    // html encode to be consistent with api endpoints that encode to protect again script injection 
                    existingWorkHistoryItem.Title = parsedJobTitle;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "Title", jobTitle, parsedJobTitle, (int)ResumeParseStatus.Merged, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                }
                else
                {
                    string question = string.Empty;
                    if (existingWorkHistoryItem.StartDate != null && existingWorkHistoryItem.EndDate != null)
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName} from {existingWorkHistoryItem.StartDate.Value.ToShortDateString()} to {existingWorkHistoryItem.EndDate.Value.ToShortDateString()}, what was your job title?";
                    else if (existingWorkHistoryItem.StartDate != null)
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName} starting on {existingWorkHistoryItem.StartDate.Value.ToShortDateString()}, what was your job title?";
                    else if (existingWorkHistoryItem.EndDate != null)
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName} ending on {existingWorkHistoryItem.EndDate.Value.ToShortDateString()}, what was your job title?";
                    else
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName}, what was your job title?";

                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, question, "SubscriberWorkHistory", "Title", jobTitle, parsedJobTitle, (int)ResumeParseStatus.MergeNeeded, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }


            if (string.IsNullOrWhiteSpace(parsedWorkHistory.JobDescription) == false)
            {
                string jobDescription = Utils.RemoveNewlines(existingWorkHistoryItem.JobDescription.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedJobDescription = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedWorkHistory.JobDescription.Trim()));

                if (string.IsNullOrWhiteSpace(jobDescription) || jobDescription.ToLower() == parsedJobDescription.ToLower())
                {
                    existingWorkHistoryItem.JobDescription = parsedJobDescription;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "JobDescription", jobDescription, parsedJobDescription, (int)ResumeParseStatus.Merged, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                }
                else
                {
                    string question = string.Empty;
                    if (existingWorkHistoryItem.StartDate != null && existingWorkHistoryItem.EndDate != null)
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName} from {existingWorkHistoryItem.StartDate.Value.ToShortDateString()} to {existingWorkHistoryItem.EndDate.Value.ToShortDateString()}, what was your job description?";
                    else if (existingWorkHistoryItem.StartDate != null)
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName} starting on {existingWorkHistoryItem.StartDate.Value.ToShortDateString()}, what was your job description?";
                    else if (existingWorkHistoryItem.EndDate != null)
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName} ending on {existingWorkHistoryItem.EndDate.Value.ToShortDateString()}, what was your job description?";
                    else
                        question = $"While working at {existingWorkHistoryItem.Company.CompanyName}, what was your job description?";


                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, question, "SubscriberWorkHistory", "JobDescription", jobDescription, parsedJobDescription, (int)ResumeParseStatus.MergeNeeded, existingWorkHistoryItem.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }


            await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
            return requireMerge;
        }

        /// <summary>
        /// import work history information found in user's resume 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <returns></returns>
        private async Task<bool> _ImportResumeWorkHistory(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            bool requireMerge = false;
            try
            {
                // get list of work histories parsed from resume 
                List<SubscriberWorkHistoryDto> parsedWorkHistory = Utils.ParseWorkHistoryFromHrXml(resume);
                // get list of work histories alreay associated with user 
                IList<SubscriberWorkHistory> workHistory = await SubscriberFactory.GetSubscriberWorkHistoryById(_repository, subscriber.SubscriberId);
                foreach (SubscriberWorkHistoryDto parsedWorkHistoryItem in parsedWorkHistory)
                {
                    // ignore jobs without job titles - some resumes such as Jon Foley's have a header section with a summary of their
                    // career at a given company and then list specific job roles below this header. If this summary is included it would
                    // create conficts for each specific job role since there would be an overlap in time at the same company.  Best to 
                    // just ignore those jobs without a job title

                    if (string.IsNullOrEmpty(parsedWorkHistoryItem.Title))
                        continue;
                    // get the company name of the company from the parsed work history 
                    string parsedCompanyName = parsedWorkHistoryItem.Company.ToLower();
                    // get a list of existing work histories for the parsed company
                    var ExistingPositions = workHistory.Where(s => s.Company.CompanyName.ToLower() == parsedCompanyName).ToList();
                    //  look for an existing position at the parsed company that overlaps in time with the parsed position 
                    var ExistingPosition = FindOverlappingWorkHistory(ExistingPositions, parsedWorkHistoryItem);
                    // get or create the company specified by the work history 
                    Company company = await CompanyFactory.GetOrAdd(_repository, parsedCompanyName);
                    // if its not an existing company just add it
                    if (ExistingPosition == null)
                    {
                        // easy case of adding new company to user 
                        SubscriberWorkHistory newWorkHistory = await SubscriberWorkHistoryFactory.AddWorkHistoryForSubscriber(_repository, subscriber, parsedWorkHistoryItem, company);
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "Object", string.Empty, string.Empty, (int)ResumeParseStatus.Merged, newWorkHistory.SubscriberWorkHistoryGuid);
                        // add new work history to existing work histories 
                        workHistory.Add(newWorkHistory);
                    }
                    else // check for conflicts with an existing position
                    {
                        if (await MergeWorkHistories(resumeParse, ExistingPosition, parsedWorkHistoryItem) == true)
                            requireMerge = true;
                    }
                }
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
                return requireMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeWorkHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }

        }



        private SubscriberWorkHistory FindOverlappingWorkHistory(List<SubscriberWorkHistory> existingWorkHistoriesForCompany, SubscriberWorkHistoryDto parsedWorkHistoryForCompany)
        {
            DateTime startDate = parsedWorkHistoryForCompany.StartDate == null ? DateTime.MinValue : parsedWorkHistoryForCompany.StartDate.Value;
            DateTime endDate = parsedWorkHistoryForCompany.EndDate == null ? DateTime.MinValue : parsedWorkHistoryForCompany.EndDate.Value;

            foreach (SubscriberWorkHistory workHistory in existingWorkHistoriesForCompany)
            {
                if (startDate == workHistory.StartDate &&
                     endDate == workHistory.EndDate &&
                     workHistory.Title.Trim().ToLower() == parsedWorkHistoryForCompany.Title.Trim().ToLower()
                    )
                {

                    return workHistory;
                }
            }
            return null;
        }

        /// <summary>
        /// Import skills found in the resume 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <returns></returns>

        private async Task<bool> _ImportResumeSkills(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            try
            {
                bool requireMerge = false;
                List<string> parsedSkills = Utils.ParseSkillsFromHrXML(resume);
                IList<SubscriberSkill> skills = await SubscriberFactory.GetSubscriberSkillsById(_repository, subscriber.SubscriberId);
                HashSet<string> foundSkills = new HashSet<string>();
                foreach (string skillName in parsedSkills)
                {


                    string parsedSkill = skillName.ToLower();
                    // were getting back duplicate skills from sovren, so we'll ignore the dupes
                    if (foundSkills.Contains(parsedSkill) == false)
                    {
                        foundSkills.Add(parsedSkill);
                        int status = (int)ResumeParseStatus.MergeNeeded;
                        string prompt = "Do you have the following skill?";
                        var existingSkill = skills.Where(s => s.Skill.SkillName.ToLower() == parsedSkill).FirstOrDefault();
                        if (existingSkill != null)
                        {
                            prompt = string.Empty;
                            status = (int)ResumeParseStatus.Duplicate;
                        }
                        else
                            requireMerge = true;

                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.Skills, prompt, "SubscriberSkill", "SkillName", parsedSkill, parsedSkill, status, subscriber.SubscriberGuid.Value);
                    }

                }
                // save resume parse results 
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
                return requireMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeSkills threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }
        }





        /// <summary>
        /// Import resume contact information 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <returns></returns>

        private async Task<bool> _ImportResumeContactInfo(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            bool requireMerge = false;
            try
            {

                SubscriberContactInfoDto contactInfo = Utils.ParseContactInfoFromHrXML(resume);

                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.FirstName) || subscriber.FirstName.Trim() == contactInfo.FirstName.Trim())
                {
                    subscriber.FirstName = contactInfo.FirstName.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "FirstName", subscriber.FirstName, contactInfo.FirstName, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which first name do you prefer?", "Subscriber", "FirstName", subscriber.FirstName, contactInfo.FirstName, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }



                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.LastName) || subscriber.LastName.Trim() == contactInfo.LastName.Trim())
                {
                    subscriber.LastName = contactInfo.LastName.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "LastName", subscriber.LastName, contactInfo.LastName, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which last name do you prefer?", "Subscriber", "LastName", subscriber.LastName, contactInfo.LastName, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }


                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.City) || subscriber.City.Trim() == contactInfo.City.Trim())
                {
                    subscriber.City = contactInfo.City.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "City", subscriber.City, contactInfo.City, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "What city do you currently reside in?", "Subscriber", "City", subscriber.City, contactInfo.City, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }


                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.PostalCode) || subscriber.PostalCode.Trim() == contactInfo.PostalCode.Trim())
                {
                    subscriber.PostalCode = contactInfo.PostalCode.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "PostalCode", subscriber.PostalCode, contactInfo.PostalCode, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "What is your current postal code?", "Subscriber", "PostalCode", subscriber.PostalCode, contactInfo.PostalCode, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }



                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.Address) || subscriber.Address.Trim() == contactInfo.Address.Trim())
                {
                    subscriber.Address = contactInfo.Address.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "Address", subscriber.Address, contactInfo.Address, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which address is correct?", "Subscriber", "Address", subscriber.Address, contactInfo.Address, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }



                State state = await StateFactory.GetStateByStateCode(_repository, contactInfo.State);
                if (state != null)
                {
                    if (subscriber.StateId <= 0 || subscriber.State == null || state.StateId == subscriber.StateId)
                    {
                        subscriber.StateId = state.StateId;
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber.StateCode", "StateCode", subscriber.State.Code, contactInfo.State, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                    }
                    else
                    {
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "In which state/province do you currently reside?", "Subscriber.StateCode", "StateCode", subscriber.State.Code, contactInfo.State, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                        requireMerge = true;
                    }

                }

                // remove extraneous characters from parsed phone number 
                contactInfo.PhoneNumber = contactInfo.PhoneNumber.Trim().Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                Regex phoneRegex = new Regex(@"^([0-9]{0,3})?[2-9]{1}[0-9]{9}$");
                // validate that the parsed phone number is valid 
                if (phoneRegex.IsMatch(contactInfo.PhoneNumber))
                {
                    if (string.IsNullOrWhiteSpace(subscriber.PhoneNumber) || subscriber.PhoneNumber.Trim() == contactInfo.PhoneNumber)
                    {
                        subscriber.PhoneNumber = contactInfo.PhoneNumber.Trim();
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "PhoneNumber", subscriber.PhoneNumber, contactInfo.PhoneNumber, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                    }
                    else
                    {
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which phone number do you prefer?", "Subscriber", "PhoneNumber", subscriber.PhoneNumber, contactInfo.PhoneNumber, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                        requireMerge = true;
                    }


                }

                // save the subscriber
                await _repository.SubscriberRepository.SaveAsync();
                // save resume parse results 
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();

                return requireMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeContactInfo threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }
        }


        #endregion

        public async Task<IList<Subscriber>> GetSubscribersInGroupAsync(Guid? GroupGuid)
        {
            IEnumerable<UpDiddyApi.Models.Group> ieGroup = await _repository.GroupRepository.GetByConditionAsync(g => g.GroupGuid == GroupGuid);
            UpDiddyApi.Models.Group group = ieGroup.FirstOrDefault();

            if (group != null)
                return await _repository.SubscriberGroupRepository.GetSubscribersAssociatedWithGroupAsync(group.GroupId);
            else
                return null;
        }

        public async Task<Subscriber> GetSubscriber(ODataQueryOptions<Subscriber> options)
        {
            var queryableSubscriber = options.ApplyTo(_repository.SubscriberRepository.GetAllSubscribersAsync());
            var subscriberList = await queryableSubscriber.Cast<Subscriber>().Where(s => s.IsDeleted == 0).ToListAsync();

            return subscriberList.Count > 0 ? subscriberList[0] : null;
        }

        public async Task TrackSubscriberSignIn(Guid subscriberGuid)
        {
            _hangfireService.Enqueue<ScheduledJobs>(j => j.TrackSubscriberSignIn(subscriberGuid));
        }

        public async Task SyncAuth0UserId(Guid subscriberGuid, string auth0UserId)
        {
            _hangfireService.Enqueue<ScheduledJobs>(j => j.SyncAuth0UserId(subscriberGuid, auth0UserId));
        }

        public async Task<SubscriberSearchResultDto> SearchSubscribersAsync(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*")
        {
            DateTime startSearch = DateTime.Now;
            SubscriberSearchResultDto searchResults = new SubscriberSearchResultDto();

            string searchServiceName = _configuration["AzureSearch:SearchServiceName"];
            string adminApiKey = _configuration["AzureSearch:SearchServiceQueryApiKey"];
            string subscriberIndexName = _configuration["AzureSearch:SubscriberIndexName"];

            // map descending to azure search sort syntax of "asc" or "desc"  default is ascending so only map descending 
            string orderBy = sort;
            if (order == "descending")
                orderBy = orderBy + " desc";
            List<String> orderByList = new List<string>();
            orderByList.Add(orderBy);

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            // Create an index named hotels
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(subscriberIndexName);

            SearchParameters parameters;
            DocumentSearchResult<SubscriberInfoDto> results;

            parameters =
                new SearchParameters()
                {
                    Top = limit,
                    Skip = offset,
                    OrderBy = orderByList,
                    IncludeTotalResultCount = true,
                };
 
            results = indexClient.Documents.Search<SubscriberInfoDto>(keyword, parameters);
      

            DateTime startMap = DateTime.Now;
            searchResults.Subscribers = results?.Results?
                .Select(s => (SubscriberInfoDto)s.Document)
                .ToList();
            
            searchResults.TotalHits = results.Count.Value;
            searchResults.PageSize = limit;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.SubscriberCount = searchResults.Subscribers.Count;
            searchResults.PageNum = (offset / limit) + 1;
            
            DateTime stopMap = DateTime.Now;

            // calculate search timing metrics 
            TimeSpan intervalTotalSearch = stopMap - startSearch;
            TimeSpan intervalSearchTime = startMap - startSearch;
            TimeSpan intervalMapTime = stopMap - startMap;

            // assign search metrics to search results 
            searchResults.SearchTimeInMilliseconds = intervalTotalSearch.TotalMilliseconds;
            searchResults.SearchQueryTimeInTicks = intervalSearchTime.Ticks;
            searchResults.SearchMappingTimeInTicks = intervalMapTime.Ticks;
            
            return searchResults;
        }




    }
}