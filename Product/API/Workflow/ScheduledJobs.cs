using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyLib.Dto;
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;
using Hangfire;
using System.Net.Http;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Factory;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using UpDiddyApi.Helpers.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Helpers.Job;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.Workflow
{
    public class ScheduledJobs : BusinessVendorBase
    {
        ICloudStorage _cloudStorage;
        ISysEmail _sysEmail;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public ScheduledJobs(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysEmail sysEmail, IHttpClientFactory httpClientFactory, ILogger<ScheduledJobs> logger, ISovrenAPI sovrenApi, IHubContext<ClientHub> hub, IDistributedCache distributedCache, ICloudStorage cloudStorage, IRepositoryWrapper repositoryWrapper)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["Woz:ApiUrl"];
            _accessToken = configuration["Woz:AccessToken"];
            _syslog = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _sovrenApi = sovrenApi;
            _cloudStorage = cloudStorage;
            _hub = hub;
            _cache = distributedCache;
            _sysEmail = sysEmail;
            _repositoryWrapper = repositoryWrapper;
        }

        #region Marketing

        public async Task<bool> SendWelcomeEmail(Guid partnerContactGuid, string firstName, string lastName, string email)
        {
            // retrieve the unique identifier for the lead and campaign
            var tinyId = _db.CampaignPartnerContact.Where(cpc => cpc.PartnerContact.PartnerContactGuid == partnerContactGuid && cpc.Campaign.Name == "PPL Lead Gen").FirstOrDefault()?.TinyId;

            // dynamic data should include: first/last name, tinyId (which can be used to infer campaign, partner contact, and view)
            var templateData = new
            {
                firstName = firstName,
                lastName = lastName,
                tinyId = tinyId
            };

            // send templated welcome email that links to custom landing page
            _sysEmail.SendTemplatedEmailAsync(email, _configuration["SysEmail:TemplateIds:LeadIntake-WelcomeEmail"].ToString(), templateData, null);

            return true;
        }

        #endregion

        #region Woz


        public bool UpdateWozStudentLastLogin(string SubscriberGuid)
        {
            try
            {

                _syslog.Log(LogLevel.Information, $"***** UpdateWozStudentLastLogin started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid.ToString()}");
                Subscriber subscriber = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == Guid.Parse(SubscriberGuid))
                .FirstOrDefault();

                if (subscriber == null)
                    return false;

                Vendor woz = _db.Vendor
                    .Where(v => v.IsDeleted == 0 && v.Name == Constants.WozVendorName)
                    .FirstOrDefault();

                if (woz == null)
                    return false;

                int WozVendorId = woz.VendorId;
                VendorStudentLogin studentLogin = _db.VendorStudentLogin
                    .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriber.SubscriberId && s.VendorId == WozVendorId)
                    .FirstOrDefault();

                if (studentLogin == null)
                    return false;

                DateTime? LastLoginDate = GetWozStudentLastLogin(int.Parse(studentLogin.VendorLogin));
                if (LastLoginDate != null && (studentLogin.LastLoginDate == null || LastLoginDate > studentLogin.LastLoginDate))
                {
                    studentLogin.LastLoginDate = LastLoginDate;
                    _db.SaveChanges();
                }

                _syslog.Log(LogLevel.Information, $"***** UpdateWozStudentLastLogin completed at: {DateTime.UtcNow.ToLongDateString()}");
                return true;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "UpdateWozStudentLastLogin:GetWozCourseProgress threw an exception -> " + e.Message);
                return false;
            }

        }


        // batch job to call woz to get students course progress
        // todo consider some form of rate limiting once we have numerous enrollments
        public bool UpdateAllStudentsProgress()
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** UpdateAllStudentsProgress started at: {DateTime.UtcNow.ToLongDateString()}");
                // get all enrollments that are not complete
                List<Enrollment> enrollments = _db.Enrollment
                    .Include(s => s.Subscriber)
                    .Where(e => e.IsDeleted == 0 && e.PercentComplete < 100)
                    .ToList();

                bool updatesMade = false;
                foreach (Enrollment e in enrollments)
                {
                    if (e.Subscriber != null)
                        UpdateWozEnrollment(e.Subscriber.SubscriberGuid.ToString(), e, ref updatesMade);
                }

                if (updatesMade)
                    _db.SaveChanges();

            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "ScheduledJobs:UpdateAllStudentsProgress threw an exception -> " + e.Message);
                throw e;
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** UpdateAllStudentsProgress completed at: {DateTime.UtcNow.ToLongDateString()}");
            }

            return true;

        }



        public bool UpdateStudentProgress(string SubscriberGuid, int ProgressUpdateAgeThresholdInHours)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress started for subscriber {SubscriberGuid.ToString()} ProgressUpdateAgeThresholdInHours = {ProgressUpdateAgeThresholdInHours}");
                Subscriber subscriber = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == Guid.Parse(SubscriberGuid))
                .FirstOrDefault();

                if (subscriber == null)
                    return false;

                IList<Enrollment> enrollments = _db.Enrollment
                    .Where(e => e.IsDeleted == 0 && e.SubscriberId == subscriber.SubscriberId && e.CompletionDate == null && e.DroppedDate == null)
                    .ToList();

                WozCourseProgressDto wcp = null;
                bool updatesMade = false;

                foreach (Enrollment e in enrollments)
                {
                    _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress looking to update enrollment {e.EnrollmentGuid}");
                    // Only Call woz if the modify date is null or if the modify date older that progress update age threshold
                    if (e.ModifyDate == null || ((DateTime)e.ModifyDate).AddHours(ProgressUpdateAgeThresholdInHours) <= DateTime.UtcNow)
                    {
                        _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress calling woz for enrollment {e.EnrollmentGuid}");
                        UpdateWozEnrollment(SubscriberGuid, e, ref updatesMade);
                    }
                    else
                    {
                        DateTime ModifyDate = (DateTime)e.ModifyDate;
                        DateTime DateThreshold = ((DateTime)e.ModifyDate).AddHours(ProgressUpdateAgeThresholdInHours);
                        _syslog.Log(LogLevel.Information,
                            $"***** UpdateStudentProgress skipping  update for enrollment {e.EnrollmentGuid} enrollment Modify date is {ModifyDate.ToLongDateString()} {ModifyDate.ToLongTimeString()} Threshold date is {DateThreshold.ToLongDateString()} {DateThreshold.ToLongTimeString()}");
                    }
                }
                if (updatesMade)
                    _db.SaveChanges();

                _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress completed");
                return true;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, $"UpdateStudentProgress:GetWozCourseProgress threw an exception -> {e.Message} for subscriber {SubscriberGuid}");
                return false;
            }

        }




        public Boolean ReconcileFutureEnrollments()
        {
            bool result = false;
            try
            {
                _syslog.Log(LogLevel.Information, $"***** ReconcileFutureEnrollments started at: {DateTime.UtcNow.ToLongDateString()}");
                int MaxReconcilesToProcess = 10;
                int.TryParse(_configuration["Woz:MaxReconcilesToProcess"], out MaxReconcilesToProcess);

                IList<Enrollment> Enrollments = _db.Enrollment
                          .Where(t => t.IsDeleted == 0 && t.EnrollmentStatusId == (int)EnrollmentStatus.FutureRegisterStudentComplete)
                         .ToList<Enrollment>();

                WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
                foreach (Enrollment e in Enrollments)
                {
                    wi.ReconcileFutureEnrollment(e.EnrollmentGuid.ToString());
                    if (--MaxReconcilesToProcess == 0)
                        break;
                }
                result = true;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "ScheduledJobs:ReconcileFutureEnrollments threw an exception -> " + e.Message);
                throw e;
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** ReconcileFutureEnrollments completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
            return result;
        }

        #endregion


        #region Woz Private Helper Functions



        private void AssociateCampaignToCourseCompletion(string SubscriberGuid, Enrollment e)
        {
            Guid parsedSubscriberGuid;
            Guid.TryParse(SubscriberGuid, out parsedSubscriberGuid);

            var partnerContact = _db.PartnerContact
                .Include(pc => pc.Contact).ThenInclude(c => c.Subscriber)
                .Where(pc => pc.IsDeleted == 0 && pc.Contact.IsDeleted == 0 && pc.Contact.Subscriber.SubscriberGuid.HasValue && pc.Contact.Subscriber.SubscriberGuid.Value == parsedSubscriberGuid)
                .OrderByDescending(pc => pc.CreateDate)
                .FirstOrDefault();

            // if there is an associated contact record for this subscriber and a campaign association for the enrollment, record that they completed the course
            if (partnerContact != null && e.CampaignId.HasValue)
            {
                var existingCourseCompletionAction = _db.PartnerContactAction
                    .Where(pca => pca.PartnerContactId == partnerContact.PartnerContactId && pca.CampaignId == e.CampaignId && pca.ActionId == 5)
                    .FirstOrDefault();

                if (existingCourseCompletionAction != null)
                {
                    // update if the action already exists (possible if more than one course was offered for a single campaign)
                    existingCourseCompletionAction.ModifyDate = DateTime.UtcNow;
                    existingCourseCompletionAction.OccurredDate = DateTime.UtcNow;
                }
                else
                {
                    // create if the action does not already exist, and associate it with the highest phase                   
                    // of the campaign that the user has interacted with and assume that is what led them to buy
                    CampaignPhase lastCampaignPhaseInteraction = CampaignPhaseFactory.GetContactsLastPhaseInteraction(_db, e.CampaignId.Value, partnerContact.ContactId);
                    if (lastCampaignPhaseInteraction != null)
                    {
                        _db.PartnerContactAction.Add(new PartnerContactAction()
                        {
                            ActionId = 5, // todo: this should not be a hard-coded reference to the PK
                            CampaignId = e.CampaignId.Value,
                            PartnerContactActionGuid = Guid.NewGuid(),
                            PartnerContactId = partnerContact.PartnerContactId,
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            IsDeleted = 0,
                            ModifyDate = DateTime.UtcNow,
                            ModifyGuid = Guid.Empty,
                            OccurredDate = DateTime.UtcNow,
                            CampaignPhaseId = lastCampaignPhaseInteraction.CampaignPhaseId
                        });
                    }
                }
            }
        }


        // check to see if student has completed the course and set completion date
        private void UpdateWozCourseCompletion(string SubscriberGuid, Enrollment e)
        {
            // Set the enrollments course completion date if the course has been completed and 
            // a completion date has not been noted
            if (e.PercentComplete == 100 && e.CompletionDate == null)
            {
                e.CompletionDate = DateTime.UtcNow;
                // See if we can 
                AssociateCampaignToCourseCompletion(SubscriberGuid, e);
            }

        }

        // update the subscribers woz course progress 
        private void UpdateWozEnrollment(string SubscriberGuid, Enrollment e, ref bool updatesMade)
        {
            WozCourseProgressDto wcp = null;
            wcp = GetWozCourseProgress(e);
            if (wcp != null && wcp.ActivitiesCompleted > 0 && wcp.ActivitiesTotal > 0)
            {
                _syslog.Log(LogLevel.Information, $"***** UpdateWozEnrollment updating enrollment {e.EnrollmentGuid}");
                int PercentComplete = Convert.ToInt32(((double)wcp.ActivitiesCompleted / (double)wcp.ActivitiesTotal) * 100);
                // update the percent completion if it's changed 
                if (e.PercentComplete != PercentComplete)
                {
                    e.PercentComplete = PercentComplete;
                    updatesMade = true;
                    UpdateWozCourseCompletion(SubscriberGuid, e);
                    _syslog.Log(LogLevel.Information, $"***** UpdateWozEnrollment updating enrollment {e.EnrollmentGuid} set PercentComplete={e.PercentComplete}");
                    e.ModifyDate = DateTime.UtcNow;
                }
                else
                    _syslog.Log(LogLevel.Information, $"***** UpdateWozEnrollment no progress for {e.EnrollmentGuid}");
            }
            else
            {
                if (wcp == null)
                    _syslog.Log(LogLevel.Information, $"***** UpdateWozEnrollment GetWozCourseProgress returned null for enrollment {e.EnrollmentGuid}");
                else
                    _syslog.Log(LogLevel.Information, $"***** UpdateWozEnrollment GetWozCourseProgress returned ActivitiesCompleted = {wcp.ActivitiesCompleted} ActivitiesTotal = {wcp.ActivitiesTotal}");
            }

        }


        // call woz to get the students progress on their woz course 
        private WozCourseProgressDto GetWozCourseProgress(Enrollment enrollment)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** GetWozCourseProgress started at: {DateTime.UtcNow.ToLongDateString()} for enrollment {enrollment.EnrollmentGuid.ToString()}");
                WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
                WozCourseEnrollment wce = _db.WozCourseEnrollment
                .Where(
                       t => t.IsDeleted == 0 &&
                       t.EnrollmentGuid == enrollment.EnrollmentGuid
                       )
                .FirstOrDefault();

                if (wce == null)
                    return null;

                WozCourseProgressDto Wcp = wi.GetCourseProgress(wce.SectionId, wce.WozEnrollmentId).Result;
                _syslog.Log(LogLevel.Information, $"***** GetWozCourseProgress completed at: {DateTime.UtcNow.ToLongDateString()}");
                return Wcp;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "ScheduledJobs:GetWozCourseProgress threw an exception -> " + e.Message);
                return null;
            }
        }


        private DateTime? GetWozStudentLastLogin(int exeterId)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** GetWozCourseProgress started at: {DateTime.UtcNow.ToLongDateString()} for woz login {exeterId}");
                WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
                WozStudentInfoDto studentLogin = wi.GetStudentInfo(exeterId).Result;
                if (studentLogin == null)
                    return null;
                else
                    return studentLogin.LastLoginDate;


            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "ScheduledJobs:GetWozCourseProgress threw an exception -> " + e.Message);
                return null;
            }
        }



        #endregion

        #region Third Party Jobs

        public async Task<bool> JobDataMining()
        {
            var result = true;
            try
            {

                List<JobSite> jobSites = _repositoryWrapper.JobSite.GetAllJobSitesAsync().Result.ToList();

                // todo: implement parallel processing
                foreach (var jobSite in jobSites)
                {

                    IJobDataMining jobDataMining = JobDataMiningFactory.GetJobDataMiningProcess(jobSite);
                    List<JobPage> jobPages = jobDataMining.GetJobPages();
                    // common method of inserting/updating job pages


                    foreach (var jobPage in jobPages)
                    {
                        // common method of processing job pages
                    }
                }



            }
            catch (Exception e)
            {
                // todo: implement logging
                result = false;
            }
            return result;
        }

        #endregion

        #region CareerCircle Jobs 

        public async Task<bool> ImportSubscriberProfileDataAsync(SubscriberFile resume)
        {
            try
            {
                resume.Subscriber = _db.Subscriber.Where(s => s.SubscriberId == resume.SubscriberId).First();
                string errMsg = string.Empty;

                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:ImportSubscriberProfileData started at: {DateTime.UtcNow.ToLongDateString()} subscriberGuid = {resume.Subscriber.SubscriberGuid}");
                string base64EncodedString = null;
                using (var ms = new MemoryStream())
                {
                    await _cloudStorage.DownloadToStreamAsync(resume.BlobName, ms);
                    var fileBytes = ms.ToArray();
                    base64EncodedString = Convert.ToBase64String(fileBytes);

                }
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:ImportSubscriberProfileData: Finished downloading and encoding file at {DateTime.UtcNow.ToLongDateString()} subscriberGuid = {resume.Subscriber.SubscriberGuid}");

                String parsedDocument = _sovrenApi.SubmitResumeAsync(base64EncodedString).Result;
                SubscriberProfileStagingStoreFactory.Save(_db, resume.Subscriber, Constants.DataSource.Sovren, Constants.DataFormat.Xml, parsedDocument);

                // Get the list of profiles that need 
                List<SubscriberProfileStagingStore> profiles = _db.SubscriberProfileStagingStore
                .Include(p => p.Subscriber)
                .Where(p => p.IsDeleted == 0 && p.Status == (int)ProfileDataStatus.Acquired && p.Subscriber.SubscriberGuid == resume.Subscriber.SubscriberGuid)
                .ToList();

                // Import user profile data
                _ImportSubscriberProfileData(profiles);

                // Callback to client to let them know upload is complete
                ClientHubHelper hubHelper = new ClientHubHelper(_hub, _cache);
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                SubscriberDto subscriberDto = SubscriberFactory.GetSubscriber(_db, (Guid)resume.Subscriber.SubscriberGuid, _syslog, _mapper);
                hubHelper.CallClient(resume.Subscriber.SubscriberGuid,
                    Constants.SignalR.ResumeUpLoadVerb,
                    JsonConvert.SerializeObject(
                        subscriberDto,
                        new JsonSerializerSettings
                        {
                            ContractResolver = contractResolver
                        }));
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "ScheduledJobs:ImportSubscriberProfileData threw an exception -> " + e.Message);
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:ImportSubscriberProfileData completed at: {DateTime.UtcNow.ToLongDateString()}");
            }

            return true;
        }

        public Boolean DoPromoCodeRedemptionCleanup(int? lookbackPeriodInMinutes = 30)
        {
            bool result = false;
            using (_syslog.BeginScope("DoPromoCodeRedemptionCleanup"))
            {
                _syslog.LogInformation("Initiating promo code redemption cleanup");
                try
                {

                    // todo: this won't perform very well if there are many records being processed. refactor when/if performance becomes an issue
                    var abandonedPromoCodeRedemptions =
                        _db.PromoCodeRedemption
                        .Include(pcr => pcr.RedemptionStatus)
                        .Where(pcr => pcr.IsDeleted == 0 && pcr.RedemptionStatus.Name == "In Process" && pcr.CreateDate.DateDiff(DateTime.UtcNow).TotalMinutes > lookbackPeriodInMinutes)
                        .ToList();

                    foreach (PromoCodeRedemption abandonedPromoCodeRedemption in abandonedPromoCodeRedemptions)
                    {
                        abandonedPromoCodeRedemption.ModifyDate = DateTime.UtcNow;
                        abandonedPromoCodeRedemption.ModifyGuid = Guid.Empty;
                        abandonedPromoCodeRedemption.IsDeleted = 1;
                        _db.Attach(abandonedPromoCodeRedemption);
                    }

                    _db.SaveChanges();
                    _syslog.LogInformation("Saved promo code redemption cleanup");

                    result = true;
                }
                catch (Exception e)
                {
                    // todo: create event ids
                    _syslog.LogError(default(EventId), e, "Error ocurred during processing");
                    throw e;
                }
            }
            return result;
        }

        public Boolean DeactivateCampaignPartnerContacts()
        {
            bool result = false;
            using (_syslog.BeginScope("DeactivateCampaignPartnerContacts"))
            {
                _syslog.LogInformation("Beginning deactivation of old campaign partner contacts");
                try
                {
                    List<CampaignPartnerContact> campaignPartnerContacts = _db.CampaignPartnerContact
                        .Where(cpc => cpc.IsDeleted == 0 && cpc.CreateDate.DateDiff(DateTime.UtcNow).TotalDays > 60)
                        .ToList();

                    foreach (CampaignPartnerContact campaignPartnerContact in campaignPartnerContacts)
                    {
                        campaignPartnerContact.ModifyDate = DateTime.UtcNow;
                        campaignPartnerContact.ModifyGuid = Guid.Empty;
                        campaignPartnerContact.TinyId = null;
                        _db.Attach(campaignPartnerContact);
                    }
                    _db.SaveChanges();
                    _syslog.LogInformation($"Deactivated {campaignPartnerContacts.Count} campaign partner contacts");

                    result = true;
                }
                catch (Exception e)
                {
                    _syslog.Log(LogLevel.Error, "ScheduledJobs:DeactivateCampaignPartnerContacts threw an exception -> " + e.Message);
                }
            }
            return result;
        }

        public void StoreTrackingInformation(Guid campaignGuid, Guid partnerContactGuid, Guid actionGuid, string campaignPhaseName, string headers)
        {
            var campaignEntity = _db.Campaign.Where(c => c.CampaignGuid == campaignGuid && c.IsDeleted == 0).FirstOrDefault();
            var partnerContactEntity = _db.PartnerContact.Where(pc => pc.PartnerContactGuid == partnerContactGuid && pc.IsDeleted == 0).FirstOrDefault();
            var actionEntity = _db.Action.Where(a => a.ActionGuid == actionGuid && a.IsDeleted == 0).FirstOrDefault();

            // validate that the referenced entities exist
            if (campaignEntity != null && partnerContactEntity != null && actionEntity != null)
            {
                // locate the campaign phase (if one exists - not required)
                CampaignPhase campaignPhase = CampaignPhaseFactory.GetCampaignPhaseByNameOrInitial(_db, campaignEntity.CampaignId, campaignPhaseName);

                // look for an existing contact action               
                var existingPartnerContactAction = PartnerContactActionFactory.GetPartnerContactAction(_db, campaignEntity, partnerContactEntity, actionEntity, campaignPhase);

                if (existingPartnerContactAction != null)
                {
                    // update the existing tracking event with a new occurred date
                    existingPartnerContactAction.ModifyDate = DateTime.UtcNow;
                    existingPartnerContactAction.OccurredDate = DateTime.UtcNow;
                    if (!string.IsNullOrWhiteSpace(headers))
                        existingPartnerContactAction.Headers = headers;
                }
                else
                {
                    // create a unique record for the tracking event
                    _db.PartnerContactAction.Add(new PartnerContactAction()
                    {
                        ActionId = actionEntity.ActionId,
                        CampaignId = campaignEntity.CampaignId,
                        PartnerContactId = partnerContactEntity.PartnerContactId,
                        PartnerContactActionGuid = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        ModifyDate = DateTime.UtcNow,
                        ModifyGuid = Guid.Empty,
                        OccurredDate = DateTime.UtcNow,
                        Headers = !string.IsNullOrWhiteSpace(headers) ? headers : null,
                        CampaignPhaseId = campaignPhase.CampaignPhaseId
                    });
                }
                _db.SaveChanges();
            }
        }

        #endregion

        #region CareerCircle  Helper Functions

        private Boolean _ImportSubscriberProfileData(List<SubscriberProfileStagingStore> profiles)
        {

            try
            {
                string errMsg = string.Empty;

                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:_ImportSubscriberProfileData started at: {DateTime.UtcNow.ToLongDateString()}");

                foreach (SubscriberProfileStagingStore p in profiles)
                {
                    if (p.ProfileSource == Constants.DataSource.LinkedIn)
                        p.Status = (int)SubscriberFactory.ImportLinkedIn(_db, _sovrenApi, p, ref errMsg);
                    else if (p.ProfileSource == Constants.DataSource.Sovren)
                        p.Status = (int)SubscriberFactory.ImportSovren(_db, p, ref errMsg, _syslog);
                    else
                    {
                        // Report on unknown source error
                        p.Status = (int)ProfileDataStatus.ProcessingError;
                        errMsg = $"ScheduledJobs:_ImportSubscriberProfileData -> SubscriberProfileStagingStore {p.SubscriberProfileStagingStoreId} has an unknown source of {p.ProfileSource}";
                    }

                    if (p.Status == (int)ProfileDataStatus.ProcessingError)
                        _syslog.Log(LogLevel.Error, $"ScheduledJobs:_ImportSubscriberProfileData envountered an error -> {errMsg}");
                }
                // Mark profiles as processed 
                _db.SaveChanges();
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:_ImportSubscriberProfileData completed at: {DateTime.UtcNow.ToLongDateString()}");

            }
            catch (Exception e)
            {
                // Save any work that has been completed before the exception 
                _db.SaveChanges();
                _syslog.Log(LogLevel.Error, "ScheduledJobs:_ImportSubscriberProfileData threw an exception -> " + e.Message);

            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:_ImportSubscriberProfileData completed at: {DateTime.UtcNow.ToLongDateString()}");
            }

            return true;
        }

        #endregion



        #region Cloud Talent

        public bool CloudTalentAddJob(Guid jobPostingGuid)
        {
            CloudTalent ct = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            ct.AddJobToCloudTalent(_db, jobPostingGuid);
            return true;
        }

        public bool CloudTalentUpdateJob(Guid jobPostingGuid)
        {
            CloudTalent ct = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            ct.UpdateJobToCloudTalent(_db, jobPostingGuid);
            return true;
        }

        public bool CloudTalentDeleteJob(Guid jobPostingGuid)
        {
            CloudTalent ct = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            ct.DeleteJobFromCloudTalent(_db, jobPostingGuid);
            return true;
        }




        #endregion




    }
}
