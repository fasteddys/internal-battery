using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
using static UpDiddyLib.Helpers.Constants;
using Newtonsoft.Json.Linq;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Data.SqlClient;
using JobPosting = UpDiddyApi.Models.JobPosting;
using UpDiddyApi.Helpers;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common;
using System.Collections.Concurrent;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.Workflow
{
    public class ScheduledJobs : BusinessVendorBase
    {
        ICloudStorage _cloudStorage;
        ISysEmail _sysEmail;
        private readonly ISubscriberService _subscriberService;
        private readonly IJobPostingService _jobPostingService;
        private readonly ITrackingService _trackingService;
        private readonly ICloudTalentService _cloudTalentService;
        private readonly IMimeMappingService _mimeMappingService;
        private readonly IHangfireService _hangfireService;
        private readonly IMemoryCache _memoryCache;
        private readonly ICourseService _courseService;
        private readonly ISitemapService _sitemapService;
        private readonly IEmploymentTypeService _employmentTypeService;
        private readonly IHiringSolvedService _hiringSolvedService;
        private readonly ISkillService _skillService;

        public ScheduledJobs(
            UpDiddyDbContext context,
            IMapper mapper,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ISysEmail sysEmail,
            IHttpClientFactory httpClientFactory,
            ILogger<ScheduledJobs> logger,
            ISovrenAPI sovrenApi,
            IHubContext<ClientHub> hub,
            IDistributedCache distributedCache,
            ICloudStorage cloudStorage,
            IRepositoryWrapper repositoryWrapper,
            ISubscriberService subscriberService,
            IJobPostingService jobPostingService,
            ITrackingService trackingService,
            IMimeMappingService mimeMappingService,
            IHangfireService hangfireService,
            ICourseService courseService,
            IMemoryCache memoryCache,
            ICloudTalentService cloudTalentService,
            ISitemapService sitemapService,
            IEmploymentTypeService employmentTypeService,
            IHiringSolvedService hiringSolvedService,
            ISkillService skillService)
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
            _trackingService = trackingService;
            _repositoryWrapper = repositoryWrapper;
            _subscriberService = subscriberService;
            _jobPostingService = jobPostingService;
            _cloudTalentService = cloudTalentService;
            _mimeMappingService = mimeMappingService;
            _hangfireService = hangfireService;
            _memoryCache = memoryCache;
            _courseService = courseService;
            _sitemapService = sitemapService;
            _employmentTypeService = employmentTypeService;
            _hiringSolvedService = hiringSolvedService;
            _skillService = skillService;
        }


        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task GenerateSiteMapAndSaveToBlobStorage()
        {
            _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.GenerateSiteMapAndSaveToBlobStorage started at: {DateTime.UtcNow.ToLongDateString()}");

            string rawBaseUrl = _configuration["Environment:BaseUrl"];
            Uri baseSiteUrl = null;
            if (!Uri.TryCreate(rawBaseUrl, UriKind.Absolute, out baseSiteUrl))
                throw new ApplicationException($"Invalid base site url specified in the API configuration: {rawBaseUrl}");

            try
            {
                var sitemap = await _sitemapService.GenerateSiteMap(baseSiteUrl);
                await _sitemapService.SaveSitemapToBlobStorage(sitemap);
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.GenerateSiteMapAndSaveToBlobStorage encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }

            _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.GenerateSiteMapAndSaveToBlobStorage completed at: {DateTime.UtcNow.ToLongDateString()}");
        }

        #region Marketing

        /// <summary>
        /// This process is responsible for delivering emails to subscribers that have unread subscriber notifications. The most
        /// content of the most recent notification should be displayed to the user as well as the number of unread notifications.
        /// Reminder emails will be sent to users who have unread notifications only on the following intervals:
        ///   - 24 hours
        ///   - 1 week
        ///   - 1 month
        /// </summary>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task SubscriberNotificationEmailReminder()
        {
            DateTime executionTime = DateTime.UtcNow;
            try
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs.SubscriberNotificationEmailReminder started at: {executionTime.ToLongDateString()}");

                var lastDay = _repositoryWrapper.NotificationRepository.GetUnreadSubscriberNotificationsForEmail(1);
                var lastSevenDays = _repositoryWrapper.NotificationRepository.GetUnreadSubscriberNotificationsForEmail(7);
                var lastThirtyDays = _repositoryWrapper.NotificationRepository.GetUnreadSubscriberNotificationsForEmail(30);
                var allReminders = lastDay.Union(lastSevenDays).Union(lastThirtyDays).ToList();

                if (allReminders != null && allReminders.Count() > 0)
                {
                    var emailInterval = TimeSpan.FromHours(24) / allReminders.Count();

                    foreach (var reminder in allReminders)
                    {

                        if (await _sysEmail.SendTemplatedEmailAsync(
                             _syslog,
                             reminder.Email,
                             _configuration["SysEmail:NotifySystem:TemplateIds:SubscriberNotification-Reminder"].ToString(),
                             new
                             {
                                 firstName = reminder.FirstName,
                                 totalUnread = reminder.TotalUnread,
                                 notificationTitle = reminder.Title,
                                 notificationsUrl = _configuration["Environment:BaseUrl"].ToString() + "dashboard",
                                 disableNotificationEmailReminders = _configuration["Environment:BaseUrl"].ToString() + "Home/DisableEmailReminders/" + reminder.SubscriberGuid
                             },
                             SendGridAccount.NotifySystem,
                             null,
                             null,
                             executionTime))
                        {
                            // only update the execution time if the email was delivered successfully
                            executionTime = executionTime.Add(emailInterval);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Critical, $"***** ScheduledJobs.SubscriberNotificationEmailReminder encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs.SubscriberNotificationEmailReminder completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
        }

        /// <summary>
        /// This process controls lead email delivery to prevent damage to our email sender reputation. This is accomplished by:
        ///     - capping the number of emails that can be delivered per hour
        ///     - mixing 'seed' emails in with the emails to be delivered 
        ///     - evenly distributes emails over the delivery window (including seed emails)
        /// </summary>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task ExecuteLeadEmailDelivery()
        {
            DateTime executionTime = DateTime.UtcNow;
            try
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs.ExecuteLeadEmailDelivery started at: {executionTime.ToLongDateString()}");

                // retrieve leads to deliver using db view
                var emailsToDeliver = _db.ThrottledLeadEmailDelivery.ToList();
                var totalEmailsToSend = emailsToDeliver.Count() + emailsToDeliver.Where(e => e.IsUseSeedEmails).Count();

                if (totalEmailsToSend > 0)
                {
                    // determine the interval use to evenly distribute the emails
                    var emailInterval = TimeSpan.FromHours(1) / totalEmailsToSend;
                    foreach (var leadEmail in emailsToDeliver)
                    {
                        if (leadEmail.IsUseSeedEmails)
                        {
                            var deliveryDate = new SqlParameter("@DeliveryDate", executionTime);
                            var spParams = new object[] { deliveryDate };
                            // retrieve the oldest and least frequently used seed email 
                            var partnerContact = _db.PartnerContact.FromSql<PartnerContact>("[dbo].[System_Get_ContactForSeedEmail] @DeliveryDate", spParams).FirstOrDefault();

                            if (
                                // send the seed email using the lead email's account and template
                                await _sysEmail.SendTemplatedEmailAsync(
                                    _syslog,
                                    partnerContact.Metadata["Email"].ToString(),
                                    leadEmail.EmailTemplateId,
                                    new
                                    {
                                        firstName = partnerContact.Metadata["FirstName"].ToString(),
                                        lastName = partnerContact.Metadata["LastName"].ToString(),
                                        timesUsed = partnerContact.Metadata["TimesUsed"].ToString()
                                    },
                                    Enum.Parse<SendGridAccount>(leadEmail.EmailSubAccountId),
                                    null,
                                    null,
                                    executionTime,
                                    leadEmail.UnsubscribeGroupId))
                            {
                                // update the execution time if the seed email was delivered successfully
                                executionTime = executionTime.Add(emailInterval);
                            }
                        }

                        bool isMailSentSuccessfully =
                         _sysEmail.SendTemplatedEmailAsync(
                            _syslog,
                            leadEmail.Email,
                            leadEmail.EmailTemplateId,
                            new
                            {
                                firstName = leadEmail.FirstName,
                                lastName = leadEmail.LastName,
                                tinyId = leadEmail.TinyId
                            },
                            Enum.Parse<SendGridAccount>(leadEmail.EmailSubAccountId),
                            null,
                            null,
                            executionTime,
                            leadEmail.UnsubscribeGroupId).Result;

                        if (isMailSentSuccessfully)
                        {
                            // retrieve the lead and campaign association record for update
                            var campaignPartnerContact =
                                _db.CampaignPartnerContact
                                .Where(cpc => cpc.CampaignId == leadEmail.CampaignId && cpc.PartnerContactId == leadEmail.PartnerContactId)
                                .FirstOrDefault();

                            // mark the lead to indicate that the email has been delivered so that we do not attempt to process it again
                            campaignPartnerContact.EmailDeliveryDate = executionTime;
                            campaignPartnerContact.IsEmailSent = true;
                            campaignPartnerContact.ModifyDate = DateTime.UtcNow;
                            campaignPartnerContact.ModifyGuid = Guid.Empty;

                            _db.SaveChanges();

                            executionTime = executionTime.Add(emailInterval);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Critical, $"***** ScheduledJobs.ExecuteLeadEmailDelivery encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs.ExecuteLeadEmailDelivery completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
        }

        /// <summary>
        /// Invokes the ZeroBounce API to verify the lead's email address. If the lead is not valid, it is updated accordingly.
        /// </summary>
        /// <param name="partnerContactGuid">The system identifier for the lead</param>
        /// <param name="email">The email address to verify</param>
        /// <param name="verificationFailureLeadStatusId">The lead status to associate with the partner contact if email validation fails</param>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public async Task ValidateEmailAddress(Guid partnerContactGuid, string email, int verificationFailureLeadStatusId)
        {
            // retrieve the partner contact that will be associated with any lead statuses we store and the log of the zero bounce request
            var partnerContact = await _db.PartnerContact.Where(pc => pc.PartnerContactGuid == partnerContactGuid).FirstOrDefaultAsync();

            if (partnerContact == null)
                throw new ApplicationException("Unrecognized partner contact");

            // verify that the email is valid using ZeroBounce
            ZeroBounceApi api = new ZeroBounceApi(_configuration, _repositoryWrapper, _syslog);
            bool? isEmailValid = api.ValidatePartnerContactEmail(partnerContact.PartnerContactId, email, verificationFailureLeadStatusId);

            // note that we are not doing anything with the result here. the responsibility for acting on this has been moved to throttled email delivery processing
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
                    .Where(v => v.IsDeleted == 0 && v.VendorGuid == Constants.WozVendorGuid)
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
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
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


        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
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



        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
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

        #region External Courses

        /// <summary>
        /// This method is responsible for the out-of-band processing for the course site crawling operation.
        /// </summary>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
        public async Task<bool> CrawlCourseSiteAsync(CourseSite courseSite)
        {
            _syslog.Log(LogLevel.Information, $"***** Crawling course site '{courseSite.Name}'. Started at: {DateTime.UtcNow.ToLongDateString()}");

            var result = true;
            try
            {
                // load the course process for the course site
                ICourseProcess courseProcess = CourseCrawlingFactory.GetCourseProcess(courseSite, _configuration, _syslog, _sovrenApi);

                // load all existing course pages - it is important to retrieve all of them regardless of their CoursePageStatus to avoid FK conflicts on insert and update operations
                var coursePages = await _repositoryWrapper.CoursePage.GetAllCoursePagesForCourseSiteAsync(courseSite.CourseSiteGuid);

                // retrieve all current course pages that are visible on the course site
                List<CoursePage> coursePagesToProcess = await courseProcess.DiscoverCoursePagesAsync(coursePages.ToList());

                // insert or update the course pages (each inserted or updated course page can have a status of: Create, Update, Delete) 
                foreach (var coursePage in coursePagesToProcess)
                {
                    try
                    {
                        if (coursePage.CoursePageId == 0)
                            await _repositoryWrapper.CoursePage.Create(coursePage);
                        else
                            _repositoryWrapper.CoursePage.Update(coursePage);

                        await _repositoryWrapper.CoursePage.SaveAsync();
                    }
                    catch (Exception e)
                    {
                        _syslog.Log(LogLevel.Critical, $"***** ScheduledJobs.CrawlCourseSiteAsync encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                    }
                }

                // update the course site to indicate that the crawl operation is complete
                courseSite.LastCrawl = DateTime.UtcNow;
                courseSite.IsCrawling = false;
                courseSite.ModifyDate = DateTime.UtcNow;
                courseSite.ModifyGuid = Guid.Empty;
                _repositoryWrapper.CourseSite.Update(courseSite);
                await _repositoryWrapper.CourseSite.SaveAsync();
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Critical, $"***** ScheduledJobs.CrawlCourseSiteAsync encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                result = false;
            }

            _syslog.Log(LogLevel.Information, $"***** Crawling course site '{courseSite.Name}'. Completed at: {DateTime.UtcNow.ToLongDateString()}");
            return result;
        }

        /// <summary>
        /// This method is responsible for the out-of-band processing for the course site syncing operation.
        /// </summary>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
        public async Task<bool> SyncCourseSiteAsync(CourseSite courseSite)
        {
            _syslog.Log(LogLevel.Information, $"***** Syncing course site '{courseSite.Name}'. Started at: {DateTime.UtcNow.ToLongDateString()}");

            var result = true;
            try
            {
                // load the course process for the course site
                ICourseProcess courseProcess = CourseCrawlingFactory.GetCourseProcess(courseSite, _configuration, _syslog, _sovrenApi);

                // load all pending course pages
                var coursePages = (await _repositoryWrapper.CoursePage.GetPendingCoursePagesForCourseSiteAsync(courseSite.CourseSiteGuid)).ToList();

                // transform course pages into courses which can be updated in the career circle schema
                ConcurrentBag<Tuple<CoursePage, CourseDto>> coursePageAndTransformedCourses = new ConcurrentBag<Tuple<CoursePage, CourseDto>>();
                var maxdop = new ParallelOptions { MaxDegreeOfParallelism = 10 };
                Parallel.For(0, coursePages.Count(), maxdop, async (index) =>
                {
                    var courseDto = await courseProcess.ProcessCoursePageAsync(coursePages[index]);
                    if (courseDto != null)
                        coursePageAndTransformedCourses.Add(new Tuple<CoursePage, CourseDto>(coursePages[index], courseDto));
                });

                // make db changes to courses and course pages
                foreach (var coursePageAndTransformedCourse in coursePageAndTransformedCourses)
                {
                    var coursePage = coursePageAndTransformedCourse.Item1;
                    var courseDto = coursePageAndTransformedCourse.Item2;

                    try
                    {
                        coursePage.CoursePageStatusId = 1; // synced                        
                        switch (coursePage.CoursePageStatus.Name)
                        {
                            case "Create":
                                coursePage.CourseId = await _courseService.AddCourseAsync(courseDto);
                                break;
                            case "Update":
                                coursePage.CourseId = await _courseService.EditCourseAsync(courseDto);
                                break;
                            case "Delete":
                                await _courseService.DeleteCourseAsync(courseDto.CourseGuid.Value);
                                coursePage.CourseId = null;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _syslog.Log(LogLevel.Error, $"***** ScheduledJobs.SyncCourseSiteAsync encountered an exception while performing a CRUD operation on a course; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                        coursePage.CoursePageStatusId = 5; // error
                    }
                    finally
                    {
                        coursePage.CoursePageStatus = null; // must do this if there is an existing status which conflicts with the value we are setting

                        // save the related course page to reflect what occurred with the course operation
                        coursePage.ModifyDate = DateTime.UtcNow;
                        coursePage.ModifyGuid = Guid.Empty;
                        _repositoryWrapper.CoursePage.Update(coursePage);
                        await _repositoryWrapper.CoursePage.SaveAsync();
                    }
                }

                // update the course site to indicate that the sync operation is complete
                courseSite.LastSync = DateTime.UtcNow;
                courseSite.IsSyncing = false;
                courseSite.ModifyDate = DateTime.UtcNow;
                courseSite.ModifyGuid = Guid.Empty;
                _repositoryWrapper.CourseSite.Update(courseSite);
                await _repositoryWrapper.CourseSite.SaveAsync();
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Critical, $"***** ScheduledJobs.SyncCourseSiteAsync encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                result = false;
            }

            _syslog.Log(LogLevel.Information, $"***** Syncing course site '{courseSite.Name}'. Completed at: {DateTime.UtcNow.ToLongDateString()}");
            return result;
        }

        #endregion

        #region Third Party Jobs

        /// <summary>
        /// This is the entry point for all third party job data mining.
        /// </summary>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 60 * 24)]
        public async Task<bool> JobDataMining()
        {
            _syslog.Log(LogLevel.Information, $"***** JobDataMining started at: {DateTime.UtcNow.ToLongDateString()}");
            string jobSiteName = string.Empty;
            string position = string.Empty;
            var result = true;
            try
            {
                IEnumerable<JobSite> jobSites = await _repositoryWrapper.JobSite.GetAllJobSitesAsync();

                foreach (var jobSite in jobSites)
                {
                    jobSiteName = jobSite.Name;
                    // initialize stat tracking for operation
                    JobSiteScrapeStatistic jobDataMiningStats =
                        new JobSiteScrapeStatistic()
                        {
                            ScrapeDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            IsDeleted = 0,
                            JobSiteId = jobSite.JobSiteId,
                            JobSiteScrapeStatisticGuid = Guid.NewGuid(),
                            NumJobsAdded = 0,
                            NumJobsDropped = 0,
                            NumJobsErrored = 0,
                            NumJobsProcessed = 0,
                            NumJobsUpdated = 0
                        };

                    // load the job data mining process for the job site
                    IJobDataMining jobDataMining = JobDataMiningFactory.GetJobDataMiningProcess(jobSite, _configuration, _syslog, _employmentTypeService);

                    // load all existing job pages - it is important to retrieve all of them regardless of their JobPageStatus to avoid FK conflicts on insert and update operations
                    IEnumerable<JobPage> existingJobPages = await _repositoryWrapper.JobPage.GetAllJobPagesForJobSiteAsync(jobSite.JobSiteGuid);
                    position = "GetAllJobPagesForJobSiteAsyncCompleted";

                    // set the number of existing active job pages before we perform any discovery operations
                    int existingActiveJobPageCount = existingJobPages.Where(jp => jp.JobPageStatusId == 2).Count();

                    // retrieve all current job pages that are visible on the job site
                    List<JobPage> jobPagesToProcess = await jobDataMining.DiscoverJobPages(existingJobPages.ToList());
                    position = "DiscoverJobPagesCompleted";

                    // set the number of pending and active jobs discovered - this will be the future state if we continue processing this job site
                    int futurePendingAndActiveJobPagesCount = jobPagesToProcess.Where(jp => jp.JobPageStatusId == 1 || jp.JobPageStatusId == 2).Count();

                    bool isExceedsSafetyThreshold = false;
                    if (jobSite.PercentageReductionThreshold.HasValue)
                    {
                        // perform safety check to ensure we don't erase all jobs if there is an intermittent problem with a job site
                        if (existingActiveJobPageCount > 0)
                        {
                            decimal percentageShift = (decimal)futurePendingAndActiveJobPagesCount / (decimal)existingActiveJobPageCount;
                            if (percentageShift < jobSite.PercentageReductionThreshold.Value)
                                isExceedsSafetyThreshold = true;
                        }
                    }
                    if (isExceedsSafetyThreshold)
                    {
                        // save the number of discovered jobs as the number of processed jobs
                        jobDataMiningStats.NumJobsProcessed = jobPagesToProcess.Count;
                        _syslog.Log(LogLevel.Critical, $"**** ScheduledJobs.JobDataMining aborted processing for {jobSite.Name} because the number of future active jobs ({futurePendingAndActiveJobPagesCount.ToString()}) fell too far below the number of existing active jobs ({existingActiveJobPageCount.ToString()}). The safety threshold is {jobSite.PercentageReductionThreshold.Value.ToString("P2")}.");
                    }
                    else
                    {
                        // convert job pages to job postings and perform the necessary CRUD operations
                        jobDataMiningStats = await ProcessJobPages(jobDataMining, jobPagesToProcess, jobDataMiningStats);
                        position = "ProcessJobPagesCompleted";
                    }

                    // store aggregate data about operations performed by job site; set scrape date at the very end of the process
                    await _repositoryWrapper.JobSiteScrapeStatistic.Create(jobDataMiningStats);
                    await _repositoryWrapper.JobSiteScrapeStatistic.SaveAsync();
                }
            }
            catch (Exception e)
            {
                // todo: implement better logging
                _syslog.Log(LogLevel.Critical, $"***** ScheduledJobs.JobDataMining encountered an exception for {jobSiteName} after {position}; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                result = false;
            }

            _syslog.Log(LogLevel.Information, $"***** ScheduledJobs.JobDataMining completed at: {DateTime.UtcNow.ToLongDateString()}");
            return result;
        }

        /// <summary>
        /// This private method provides a common way to process job pages and perform the necessary db and Google Talent Cloud operations.
        /// </summary>
        /// <param name="jobDataMining"></param>
        /// <param name="jobPagesToProcess"></param>
        /// <returns></returns>
        private async Task<JobSiteScrapeStatistic> ProcessJobPages(IJobDataMining jobDataMining, List<JobPage> jobPagesToProcess, JobSiteScrapeStatistic jobDataMiningStats)
        {
            // JobPageStatus = 1 / 'Pending' - we want to process these (add new or update existing dbo.JobPosting)                     
            var pendingJobPages = jobPagesToProcess.Where(jp => jp.JobPageStatusId == 1).ToList();
            foreach (var jobPage in pendingJobPages)
            {
                try
                {
                    bool? isJobPostingOperationSuccessful = null;
                    Guid jobPostingGuid = Guid.Empty;
                    string errorMessage = null;
                    // convert JobPage into JobPostingDto
                    var jobPostingDto = jobDataMining.ProcessJobPage(jobPage);

                    if (string.IsNullOrWhiteSpace(jobPostingDto?.Recruiter?.Email) || jobPostingDto == null)
                    {
                        // do not attempt to process a job page if the recruiter email is empty or if we were unable to construct a jobPostingDto
                        isJobPostingOperationSuccessful = false;
                    }
                    else
                    {
                        if (jobPage.JobPostingId.HasValue)
                        {
                            // get the job posting guid
                            jobPostingGuid = JobPostingFactory.GetJobPostingById(_repositoryWrapper, jobPage.JobPostingId.Value).Result.JobPostingGuid;
                            // the factory method uses the guid property of the dto for GetJobPostingByGuidWithRelatedObjects - need to set that too
                            jobPostingDto.JobPostingGuid = jobPostingGuid;

                            // attempt to update job posting
                            JobCrudDto jobCrudDto = _mapper.Map<JobCrudDto>(jobPostingDto);
                            if (jobPostingDto.JobPostingSkills != null && jobPostingDto.JobPostingSkills.Any())
                            {
                                var jobPostingSkills = await _skillService.AddOrUpdateSkillsByName(jobPostingDto.JobPostingSkills.Select(jps => jps.SkillName).ToList());
                                await _skillService.UpdateJobPostingSkillsByGuid(jobPostingGuid, jobPostingSkills.Select(jps => jps.SkillGuid).ToList());
                            }
                            isJobPostingOperationSuccessful = await _jobPostingService.UpdateJobPosting(jobCrudDto);

                            // increment updated count in stats
                            if (isJobPostingOperationSuccessful.HasValue && isJobPostingOperationSuccessful.Value)
                                jobDataMiningStats.NumJobsUpdated += 1;
                        }
                        else
                        {
                            // we have to add/update the recruiter and the associated company - should the job posting factory encapsulate that logic?
                            Recruiter recruiter = await RecruiterFactory.GetAddOrUpdate(_repositoryWrapper, jobPostingDto.Recruiter.Email, jobPostingDto.Recruiter.FirstName, jobPostingDto.Recruiter.LastName, null, null);
                            Company company = await CompanyFactory.GetCompanyByGuid(_repositoryWrapper, jobPostingDto.Company.CompanyGuid);
                            await RecruiterCompanyFactory.GetOrAdd(_repositoryWrapper, recruiter.RecruiterId, company.CompanyId, true);

                            // attempt to create job posting
                            JobCrudDto jobCrudDto = _mapper.Map<JobCrudDto>(jobPostingDto);
                            jobPostingGuid = await _jobPostingService.CreateJobPosting(jobCrudDto); if (jobPostingDto.JobPostingSkills != null && jobPostingDto.JobPostingSkills.Any())
                            {
                                var jobPostingSkills = await _skillService.AddOrUpdateSkillsByName(jobPostingDto.JobPostingSkills.Select(jps => jps.SkillName).ToList());
                                await _skillService.UpdateJobPostingSkillsByGuid(jobPostingGuid, jobPostingSkills.Select(jps => jps.SkillGuid).ToList());
                            }
                            isJobPostingOperationSuccessful = jobPostingGuid != null && jobPostingGuid != Guid.Empty;

                            // increment added count in stats
                            if (isJobPostingOperationSuccessful.HasValue && isJobPostingOperationSuccessful.Value)
                                jobDataMiningStats.NumJobsAdded += 1;
                        }
                    }

                    if (isJobPostingOperationSuccessful.HasValue && isJobPostingOperationSuccessful.Value)
                    {
                        // indicate that the job was updated successfully and is now active
                        jobPage.JobPageStatusId = 2;

                        if (!jobPage.JobPostingId.HasValue)
                        {
                            // we have the job posting guid but not the job posting id. retrieve that so we can associate the job posting with the job page
                            var result = await JobPostingFactory.GetJobPostingByGuid(_repositoryWrapper, jobPostingGuid);
                            jobPage.JobPostingId = result?.JobPostingId;
                        }

                        // add or update the job page and save the changes
                        if (jobPage.JobPageId > 0)
                            _repositoryWrapper.JobPage.Update(jobPage);
                        else
                            await _repositoryWrapper.JobPage.Create(jobPage);
                        await _repositoryWrapper.JobPage.SaveAsync();

                    }
                    else if (isJobPostingOperationSuccessful.HasValue && !isJobPostingOperationSuccessful.Value)
                    {
                        // indicate that an error occurred and save the changes
                        jobPage.JobPageStatusId = 3;

                        if (jobPage.JobPageId > 0)
                            _repositoryWrapper.JobPage.Update(jobPage);
                        else
                            await _repositoryWrapper.JobPage.Create(jobPage);
                        await _repositoryWrapper.JobPage.SaveAsync();

                        // increment error count in stats
                        jobDataMiningStats.NumJobsErrored += 1;
                    }
                }
                catch (Exception e)
                {
                    _syslog.Log(LogLevel.Information, $"***** ScheduledJobs.ProcessJobPages add/update encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                    // remove added/modified/deleted entities that are currently in the change tracker to prevent them from being retried
                    foreach (EntityEntry entityEntry in _db.ChangeTracker.Entries().ToArray())
                    {
                        if (entityEntry != null && (entityEntry.State == EntityState.Added || entityEntry.State == EntityState.Deleted || entityEntry.State == EntityState.Modified))
                        {
                            entityEntry.State = EntityState.Detached;
                        }
                    }
                    // increment error count in stats
                    jobDataMiningStats.NumJobsErrored += 1;
                }
                finally
                {
                    // increment processed counter regardless of the outcome of the operation
                    jobDataMiningStats.NumJobsProcessed += 1;
                }
            }

            // JobPageStatus = 4 / 'Deleted' - delete dbo.JobPosting (if one exists)
            var deleteJobPages = jobPagesToProcess.Where(jp => jp.JobPageStatusId == 4).ToList();
            foreach (var jobPage in deleteJobPages)
            {
                try
                {
                    bool? isJobDeleteOperationSuccessful = null;
                    string errorMessage = null;
                    Guid jobPostingGuid = Guid.Empty;
                    // verify that there is a related job posting
                    if (jobPage.JobPostingId.HasValue)
                    {
                        // get the job posting guid
                        var jobPosting = await JobPostingFactory.GetJobPostingById(_repositoryWrapper, jobPage.JobPostingId.Value);
                        jobPostingGuid = jobPosting.JobPostingGuid;
                        // attempt to delete job posting
                        isJobDeleteOperationSuccessful = JobPostingFactory.DeleteJob(_repositoryWrapper, jobPostingGuid, ref errorMessage, _syslog, _mapper, _configuration, _hangfireService);

                        if (isJobDeleteOperationSuccessful.HasValue && isJobDeleteOperationSuccessful.Value)
                        {
                            // flag job page as deleted and save the changes
                            jobPage.JobPageStatusId = 4;
                            jobPage.JobPostingId = null;

                            if (jobPage.JobPageId > 0)
                                _repositoryWrapper.JobPage.Update(jobPage);
                            else
                                await _repositoryWrapper.JobPage.Create(jobPage);
                            await _repositoryWrapper.JobPage.SaveAsync();

                            // increment drop count in stats
                            jobDataMiningStats.NumJobsDropped += 1;
                        }
                        else if (isJobDeleteOperationSuccessful.HasValue && !isJobDeleteOperationSuccessful.Value)
                        {
                            // flag job page as error and save the changes
                            jobPage.JobPageStatusId = 3;

                            if (jobPage.JobPageId > 0)
                                _repositoryWrapper.JobPage.Update(jobPage);
                            else
                                await _repositoryWrapper.JobPage.Create(jobPage);
                            await _repositoryWrapper.JobPage.SaveAsync();

                            // increment error count in stats
                            jobDataMiningStats.NumJobsErrored += 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    _syslog.Log(LogLevel.Information, $"***** ScheduledJobs.ProcessJobPages delete/error encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                    // remove added/modified/deleted entities that are currently in the change tracker to prevent them from being retried
                    foreach (EntityEntry entityEntry in _db.ChangeTracker.Entries().ToArray())
                    {
                        if (entityEntry != null && (entityEntry.State == EntityState.Added || entityEntry.State == EntityState.Deleted || entityEntry.State == EntityState.Modified))
                        {
                            entityEntry.State = EntityState.Detached;
                        }
                    }
                }
                finally
                {
                    // increment processed counter regardless of the outcome of the operation
                    jobDataMiningStats.NumJobsProcessed += 1;
                }
            }

            return jobDataMiningStats;
        }

        #endregion

        #region Resume Parsing 
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task<bool> ImportSubscriberProfileDataAsync(Subscriber subscriber, SubscriberFile resume)
        {
            try
            {
                resume.Subscriber = subscriber;
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

                String parsedDocument = _sovrenApi.SubmitResumeAsync(subscriber.SubscriberId, base64EncodedString).Result;
                // Save profile in staging store 
                SubscriberProfileStagingStoreFactory.Save(_db, resume.Subscriber, Constants.DataSource.Sovren, Constants.DataFormat.Xml, parsedDocument);
                // Import the subscriber resume 
                ResumeParse resumeParse = await _ImportSubscriberResume(_subscriberService, resume, parsedDocument);

                // Request parse from hiring solved 
                await _hiringSolvedService.RequestParse(subscriber.SubscriberId, resume.BlobName, base64EncodedString);

                // Callback to client to let them know upload is complete
                ClientHubHelper hubHelper = new ClientHubHelper(_hub, _cache);
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                UpDiddyLib.Dto.SubscriberDto subscriberDto = await SubscriberFactory.GetSubscriber(_repositoryWrapper, (Guid)resume.Subscriber.SubscriberGuid, _syslog, _mapper);
                hubHelper.CallClient(resume.Subscriber.SubscriberGuid,
                    Constants.SignalR.ResumeUpLoadVerb,
                    JsonConvert.SerializeObject(
                        subscriberDto,
                        new JsonSerializerSettings
                        {
                            ContractResolver = contractResolver
                        }));

                hubHelper.CallClient(resume.Subscriber.SubscriberGuid,
                    Constants.SignalR.ResumeUpLoadAndParseVerb,
                    JsonConvert.SerializeObject(
                        _mapper.Map<ResumeParseDto>(resumeParse),
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


        #endregion

        #region CareerCircle Jobs 

        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public async Task CacheRelatedJobSkillMatrix()
        {
            try
            {
                await _repositoryWrapper.StoredProcedureRepository.CacheRelatedJobSkillMatrix();
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.CacheRelatedJobSkillMatrix encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public void ExecuteJobPostingAlert(Guid jobPostingAlertGuid)
        {
            try
            {
                JobPostingAlert jobPostingAlert = _repositoryWrapper.JobPostingAlertRepository.GetJobPostingAlert(jobPostingAlertGuid).Result;
                //CloudTalent cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
                JobQueryDto jobQueryDto = JsonConvert.DeserializeObject<JobQueryDto>(jobPostingAlert.JobQueryDto.ToString());
                switch (jobPostingAlert.Frequency)
                {
                    case Frequency.Daily:
                        jobQueryDto.LowerBound = DateTime.UtcNow.AddDays(-1);
                        break;
                    case Frequency.Weekly:
                        jobQueryDto.LowerBound = DateTime.UtcNow.AddDays(-7);
                        break;
                }
                jobQueryDto.UpperBound = DateTime.UtcNow;
                JobSearchResultDto jobSearchResultDto = _cloudTalentService.JobSearch(jobQueryDto, isJobPostingAlertSearch: true);
                if (jobSearchResultDto.JobCount > 0)
                {
                    dynamic templateData = new JObject();
                    templateData.description = jobPostingAlert.Description;
                    templateData.firstName = jobPostingAlert.Subscriber.FirstName;
                    templateData.jobCount = jobSearchResultDto.JobCount;
                    templateData.frequency = jobPostingAlert.Frequency.ToString();
                    templateData.jobs = JArray.FromObject(jobSearchResultDto.Jobs.ToList().Select(j => new
                    {
                        title = j.Title,
                        summary = j.JobSummary.Length <= 250 ? j.JobSummary : j.JobSummary.Substring(0, 250) + "...",
                        location = j.Location,
                        posted = j.PostingDateUTC.ToShortDateString(),
                        url = _configuration["CareerCircle:ViewJobPostingUrl"] + j.JobPostingGuid
                    }).ToList());

                    var result = _sysEmail.SendTemplatedEmailAsync(
                       _syslog,
                       jobPostingAlert.Subscriber.Email,
                       _configuration["SysEmail:NotifySystem:TemplateIds:JobPosting-SubscriberAlert"],
                       templateData,
                       SendGridAccount.NotifySystem,
                       null,
                       null).Result;
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.ExecuteJobPostingAlert encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}, jobPostingAlertGuid: {jobPostingAlertGuid.ToString()}");
            }
        }

        /// <summary>
        /// In the event that our Hangfire instance(s) get out of sync with the target environment's database, we need to ensure that all scheduled job alerts exist and will fire on the schedule 
        /// defined in the dbo.JobPostingAlert table.
        /// </summary>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public void SyncJobPostingAlertsBetweenDbAndHangfire()
        {
            try
            {
                var jobPostingAlerts = _repositoryWrapper.JobPostingAlertRepository.GetByConditionAsync(jpa => jpa.IsDeleted == 0).Result;
                foreach (JobPostingAlert jobPostingAlert in jobPostingAlerts)
                {
                    // construct the Cron schedule for job alert
                    string cronSchedule = null;

                    switch (jobPostingAlert.Frequency)
                    {
                        case Frequency.Daily:
                            cronSchedule = Cron.Daily(jobPostingAlert.ExecutionHour, jobPostingAlert.ExecutionMinute);
                            break;
                        case Frequency.Weekly:
                            cronSchedule = Cron.Weekly(jobPostingAlert.ExecutionDayOfWeek.Value, jobPostingAlert.ExecutionHour, jobPostingAlert.ExecutionMinute);
                            break;
                        default:
                            throw new NotSupportedException($"Unrecognized value for 'Frequency' parameter: {jobPostingAlert.Frequency.ToString()}");
                    }
                    _hangfireService.AddOrUpdate<ScheduledJobs>($"jobPostingAlert:{jobPostingAlert.JobPostingAlertGuid}", sj => sj.ExecuteJobPostingAlert(jobPostingAlert.JobPostingAlertGuid), cronSchedule);
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.SyncHangfireJobAlerts encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60)]
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

        [DisableConcurrentExecution(timeoutInSeconds: 60)]
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

        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public void StoreTrackingInformation(string tinyId, Guid actionGuid, string campaignPhaseName, string headers)
        {
            var trackingInfoFromTinyId = _db.CampaignPartnerContact
                .Include(cpc => cpc.Campaign)
                .Include(cpc => cpc.PartnerContact)
                .Where(cpc => cpc.IsDeleted == 0 && cpc.TinyId == tinyId)
                .FirstOrDefault();
            var actionEntity = _db.Action.Where(a => a.ActionGuid == actionGuid && a.IsDeleted == 0).FirstOrDefault();

            // validate that the referenced entities exist
            if (trackingInfoFromTinyId != null && actionEntity != null)
            {
                // locate the campaign phase (if one exists - not required)
                CampaignPhase campaignPhase = CampaignPhaseFactory.GetCampaignPhaseByNameOrInitial(_db, trackingInfoFromTinyId.Campaign.CampaignId, campaignPhaseName);

                // look for an existing contact action               
                var existingPartnerContactAction = PartnerContactActionFactory.GetPartnerContactAction(_db, trackingInfoFromTinyId.Campaign, trackingInfoFromTinyId.PartnerContact, actionEntity, campaignPhase);

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
                        CampaignId = trackingInfoFromTinyId.CampaignId,
                        PartnerContactId = trackingInfoFromTinyId.PartnerContactId,
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

        public async Task<RecruiterAction> StoreRecruiterTrackingInformation(Guid ActorGuid, Guid ActionGuid, Guid JobApplicationGuid)
        {
            var Recruiter = _db.Recruiter.Where(r => r.RecruiterGuid == ActorGuid && r.IsDeleted == 0).FirstOrDefault();
            var JobApplication = _db.JobApplication.Where(j => j.JobApplicationGuid == JobApplicationGuid && j.IsDeleted == 0).FirstOrDefault();
            var EntityType = _db.EntityType.Where(e => e.Name.Equals(Constants.TRACKING_KEY_JOB_APPLICATION) && e.IsDeleted == 0).FirstOrDefault();
            var Action = _db.Action.Where(a => a.ActionGuid == ActionGuid && a.IsDeleted == 0).FirstOrDefault();
            IRecruiterActionRepository RecruiterActionRepository = _repositoryWrapper.RecruiterActionRepository;

            // validate that the referenced entities exist
            if (Recruiter != null && JobApplication != null && Action != null && EntityType != null)
            {
                IEnumerable<RecruiterAction> RecruiterAction = await RecruiterActionRepository.GetByConditionAsync((r) =>
                        r.RecruiterId == Recruiter.RecruiterId &&
                        r.ActionId == Action.ActionId &&
                        r.EntityId == JobApplication.JobApplicationId &&
                        r.EntityTypeId == EntityType.EntityTypeId &&
                        r.IsDeleted == 0);

                RecruiterAction ExistingRecruiterAction = RecruiterAction.FirstOrDefault();

                if (ExistingRecruiterAction != null)
                {
                    ExistingRecruiterAction.ModifyDate = DateTime.UtcNow;
                    ExistingRecruiterAction.OccurredDate = DateTime.UtcNow;
                    RecruiterActionRepository.Update(ExistingRecruiterAction);
                    await RecruiterActionRepository.SaveAsync();
                    return ExistingRecruiterAction;
                }
                else
                {
                    RecruiterAction NewRecruiterAction = (new RecruiterAction()
                    {
                        IsDeleted = 0,
                        CreateDate = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        ModifyGuid = Guid.Empty,
                        RecruiterActionGuid = Guid.NewGuid(),
                        RecruiterId = Recruiter.RecruiterId,
                        ActionId = Action.ActionId,
                        OccurredDate = DateTime.UtcNow,
                        EntityId = JobApplication.JobApplicationId,
                        EntityTypeId = EntityType.EntityTypeId
                    });
                    await RecruiterActionRepository.Create(NewRecruiterAction);
                    await RecruiterActionRepository.SaveAsync();
                    return NewRecruiterAction;
                }

            }
            return null;

        }

        /// <summary>
        /// Sends email to subscribers who clicked Apply on job posting and did not click submit 
        /// </summary>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task ExecuteJobAbandonmentEmailDelivery()
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:_ExecuteJobAbandonmentEmailDelivery started at: {DateTime.UtcNow.ToLongDateString()}");
                Dictionary<Subscriber, List<JobPosting>> subscribersToJobPostingMapping = await _trackingService.GetSubscriberAbandonedJobPostingHistoryByDateAsync(DateTime.UtcNow.AddDays(-1));
                if (subscribersToJobPostingMapping.Count > 0)
                {
                    string jobPostingUrl = _configuration["CareerCircle:ViewJobPostingUrl"];
                    dynamic recruiterTemplate = new JObject();
                    foreach (KeyValuePair<Subscriber, List<JobPosting>> entry in subscribersToJobPostingMapping)
                    {
                        //Search for similar jobs
                        JobQueryDto jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(entry.Value.FirstOrDefault().Province
                            , entry.Value.FirstOrDefault().City
                            , entry.Value.FirstOrDefault().Title
                            , Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsForJobAbandonment"]));
                        JobSearchResultDto similarJobSearchResults = _cloudTalentService.JobSearch(jobQuery);

                        //Remove duplicates subscriber already attempted to apply to
                        foreach (var job in entry.Value)
                        {
                            similarJobSearchResults.Jobs.RemoveAll(x => x.JobPostingGuid == job.JobPostingGuid);
                        }

                        //Send email to subscriber
                        bool result = await _sysEmail.SendTemplatedEmailAsync(
                                   _syslog,
                                  entry.Key.Email,
                                  _configuration["SysEmail:NotifySystem:TemplateIds:JobApplication-AbandonmentAlert"],
                                  SendGridHelper.GenerateJobAbandonmentEmailTemplate(entry, similarJobSearchResults.Jobs, jobPostingUrl),
                                  SendGridAccount.NotifySystem,
                                  null,
                                  null);
                    }

                    //Send emails out to recruiters
                    var jobAbandonmentEmails = _configuration.GetSection("SysEmail:JobAbandonmentEmails").GetChildren().Select(x => x.Value).ToList();
                    foreach (string email in jobAbandonmentEmails)
                    {
                        await _sysEmail.SendTemplatedEmailAsync(
                                _syslog,
                              email,
                              _configuration["SysEmail:NotifySystem:TemplateIds:JobApplication-AbandonmentAlert-Recruiter"],
                              SendGridHelper.GenerateJobAbandonmentRecruiterTemplate(subscribersToJobPostingMapping, jobPostingUrl),
                              SendGridAccount.NotifySystem,
                              null,
                              null);
                    }
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.ExecuteJobAbandonmentEmailDelivery encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
        }


        #endregion

        #region CareerCircle  Helper Functions
        /// <summary>
        /// Import a subscriber resume 
        /// </summary>
        /// <param name="resume"></param>
        /// <param name="subscriberFileId"></param>
        /// <returns></returns>
        private async Task<ResumeParse> _ImportSubscriberResume(ISubscriberService subscriberService, SubscriberFile resumeFile, string resume)
        {

            // Delete all existing resume parses for user
            await _repositoryWrapper.ResumeParseRepository.DeleteAllResumeParseForSubscriber(resumeFile.SubscriberId);
            // Create resume parse object 
            ResumeParse resumeParse = await _repositoryWrapper.ResumeParseRepository.CreateResumeParse(resumeFile.SubscriberId, resumeFile.SubscriberFileId);


            // Import resume 
            if (await subscriberService.ImportResume(resumeParse, resume) == true)
                resumeParse.RequiresMerge = 1;

            // Save Resume Parse 
            await _repositoryWrapper.ResumeParseRepository.SaveAsync();

            return resumeParse;
        }

        private Boolean _ImportSubscriberProfileData(List<SubscriberProfileStagingStore> profiles)
        {
            try
            {
                string errMsg = string.Empty;

                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:_ImportSubscriberProfileData started at: {DateTime.UtcNow.ToLongDateString()}");

                foreach (SubscriberProfileStagingStore p in profiles)
                {
                    if (p.ProfileSource == Constants.DataSource.LinkedIn)
                        p.Status = (int)SubscriberFactory.ImportLinkedIn(_repositoryWrapper, _sovrenApi, p, ref errMsg);
                    else if (p.ProfileSource == Constants.DataSource.Sovren)
                        p.Status = (int)SubscriberFactory.ImportSovren(_repositoryWrapper, p, ref errMsg, _syslog);
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

        #region Cloud Talent Profiles 

        [DisableConcurrentExecution(timeoutInSeconds: 30)]
        public async Task<bool> CloudTalentAddOrUpdateProfile(Guid subscriberGuid)
        {
            return await _cloudTalentService.AddOrUpdateProfileToCloudTalent(subscriberGuid);
        }

        [DisableConcurrentExecution(timeoutInSeconds: 30)]
        public async Task<bool> CloudTalentDeleteProfile(Guid subscriberGuid, Guid? cloudIdentifier)
        {
            await _cloudTalentService.DeleteProfileFromCloudTalent(subscriberGuid, cloudIdentifier);
            return true;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task<bool> CloudTalentIndexNewProfiles(int numProfilesToProcess)
        {
            //CloudTalent ct = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
            int indexVersion = int.Parse(_configuration["CloudTalent:ProfileIndexVersion"]);
            List<Subscriber> subscribers = await _subscriberService.GetSubscribersToIndexIntoGoogle(numProfilesToProcess, indexVersion);
            foreach (Subscriber s in subscribers)
            {
                await _cloudTalentService.AddOrUpdateProfileToCloudTalent(s.SubscriberGuid.Value);

            }
            return true;
        }

        /// <summary>
        /// Inspects all profiles returned from a Google Talent Cloud search request. If any of those returned do not exist in the 
        /// environment's database, the record will be purged from the Google Talent Cloud platform. 
        /// </summary>
        /// <param name="profiles">A collection of profiles that were returned from a Google Talent Cloud search request</param>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task PurgeOrphanedSubscribersFromCloudTalent(List<ProfileViewDto> profiles)
        {
            if (profiles != null && profiles.Count() > 0)
                _syslog.LogInformation($"ScheduledJobs.PurgeOrphanedSubscribersFromCloudTalent: {profiles.Count} profiles being checked for orphaned subscribers.");

            foreach (var profile in profiles)
            {
                _syslog.LogInformation($"ScheduledJobs.PurgeOrphanedSubscribersFromCloudTalent: Verifying subscriber {profile.Email} in database.");
                Subscriber subscriber = null;
                if (profile.SubscriberGuid.HasValue)
                {
                    _syslog.LogInformation($"ScheduledJobs.PurgeOrphanedSubscribersFromCloudTalent: Subscriber {profile.Email} has a subscriber guid of {profile.SubscriberGuid.Value.ToString()}.");
                    subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(profile.SubscriberGuid.Value, false);

                    // Purge profiles from google that have no related subscriber record or have a deleted subscriber record
                    if ((subscriber == null || subscriber.IsDeleted == 1) && !string.IsNullOrWhiteSpace(profile.CloudTalentUri))
                    {
                        try

                        {
                            _syslog.LogInformation($"ScheduledJobs.PurgeOrphanedSubscribersFromCloudTalent: Subscriber {profile.Email} does not exist in the database, the cloud talent uri to be purged is: {profile.CloudTalentUri}");
                            var response = _cloudTalentService.DeleteProfileFromCloudTalentByUri(profile.CloudTalentUri);
                            _syslog.LogInformation($"ScheduledJobs.PurgeOrphanedSubscribersFromCloudTalent: The response from the cloud talent delete endpoint for subscriber {profile.Email} was: {response.Description}, status code: {response.StatusCode}");
                        }
                        catch (Exception e)
                        {
                            _syslog.LogInformation($"ScheduledJobs.PurgeOrphanedSubscribersFromCloudTalent: An exception occurred; message={e.Message}, source={e.Source}, stack trace={e.StackTrace}");
                        }
                    }
                }
            }
        }

        #endregion

        #region Cloud Talent Jobs 

        [DisableConcurrentExecution(timeoutInSeconds: 30)]
        public async Task<bool> CloudTalentAddJob(Guid jobPostingGuid)
        {
            await _cloudTalentService.AddJobToCloudTalent(jobPostingGuid);
            return true;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 30)]
        public async Task<bool> CloudTalentUpdateJob(Guid jobPostingGuid)
        {
            await _cloudTalentService.UpdateJobToCloudTalent(jobPostingGuid);
            return true;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 30)]
        public async Task<bool> CloudTalentDeleteJob(Guid jobPostingGuid)
        {
            await _cloudTalentService.DeleteJobFromCloudTalent(jobPostingGuid);
            return true;
        }

        #endregion

        #region Admin Portal

        /// <summary>
        /// This function creates entries in the SubscriberNotification table for each
        /// subscriber.
        /// 
        /// This function assumes the Notification is valid, and that the subscribers
        /// being sent in have not been deleted.
        /// </summary>
        /// <param name="Notification"></param>
        /// <param name="Subscribers"></param>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task<bool> CreateSubscriberNotificationRecords(Notification notification, IList<Subscriber> Subscribers = null)
        {

            if (notification.IsTargeted == 0)
                Subscribers = _repositoryWrapper.SubscriberRepository.GetByConditionAsync(s => s.IsDeleted == 0).Result.ToList();
            else
            {
                if (Subscribers == null || Subscribers.Count <= 0)
                    return false;
            }


            IList<SubscriberNotification> SubscriberNotifications = new List<SubscriberNotification>();
            foreach (Subscriber sub in Subscribers)
            {
                DateTime CurrentDateTime = DateTime.UtcNow;
                SubscriberNotification subscriberNotification = new SubscriberNotification
                {
                    SubscriberNotificationGuid = Guid.NewGuid(),
                    SubscriberId = sub.SubscriberId,
                    NotificationId = notification.NotificationId,
                    CreateDate = CurrentDateTime,
                    ModifyDate = CurrentDateTime,
                    HasRead = 0
                };
                SubscriberNotifications.Add(subscriberNotification);
            }
            await _repositoryWrapper.SubscriberNotificationRepository.CreateRange(SubscriberNotifications.ToArray());
            await _repositoryWrapper.SubscriberNotificationRepository.SaveAsync();

            return true;
        }

        /// <summary>
        /// Mark each entry within the SubscriberNotification table as deleted for
        /// the Notification being passed in.
        /// </summary>
        /// <param name="Notification"></param>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task<bool> DeleteSubscriberNotificationRecords(Notification Notification)
        {
            if (Notification == null)
                return false;

            IList<SubscriberNotification> subscriberNotifications = _repositoryWrapper.SubscriberNotificationRepository.GetByConditionAsync(
                n => n.NotificationId == Notification.NotificationId && n.IsDeleted == 0).Result.ToList();

            if (subscriberNotifications.Count <= 0)
                return false;

            IList<SubscriberNotification> SubscriberNotifications = new List<SubscriberNotification>();

            foreach (SubscriberNotification subscriberNotification in subscriberNotifications)
            {
                subscriberNotification.IsDeleted = 1;
                subscriberNotification.ModifyDate = DateTime.UtcNow;

                SubscriberNotifications.Add(subscriberNotification);
            }

            _repositoryWrapper.SubscriberNotificationRepository.UpdateRange(SubscriberNotifications.ToArray());
            await _repositoryWrapper.SubscriberNotificationRepository.SaveAsync();

            return true;
        }

        #endregion

        #region Subscriber tracking 

        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public async Task TrackSubscriberActionInformation(Guid subscriberGuid, Guid actionGuid, Guid entityTypeGuid, Guid entityGuid)
        {

            try
            {
                // load the subscriber
                Subscriber subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);

                if (subscriber == null)
                    throw new InvalidOperationException("Subscriber not found");

                // load the action
                var action = await _repositoryWrapper.ActionRepository.GetActionByActionGuid(actionGuid);
                if (action == null)
                    throw new InvalidOperationException("Action not found");

                // load the related entity associated with the action (only if specified)
                EntityType entityType = null;
                int? entityId = null;
                if (entityGuid != Guid.Empty)
                {
                    // load the entity type
                    entityType = await _repositoryWrapper.EntityTypeRepository.GetEntityTypeByEntityGuid(entityTypeGuid);
                    if (entityType == null)
                        throw new InvalidOperationException("Entity type not found");

                    switch (entityType.Name)
                    {
                        case "Subscriber":
                            var subscriberEntity = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(entityGuid);
                            if (subscriberEntity == null)
                                throw new InvalidOperationException("Related subscriber entity not found");
                            entityId = subscriberEntity.SubscriberId;
                            break;
                        case "Offer":
                            var offerEntity = await _repositoryWrapper.Offer.GetOfferByOfferGuid(entityGuid);
                            if (offerEntity == null)
                                throw new InvalidOperationException("Related offer entity not found");
                            entityId = offerEntity.OfferId;
                            break;
                        case "Job posting":
                            var jobPosting = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(entityGuid);
                            if (jobPosting == null)
                                throw new InvalidOperationException("Related jobPosting entity not found");
                            entityId = jobPosting.JobPostingId;
                            break;
                        default:
                            throw new NotSupportedException("Unrecognized entity type");
                    }
                }

                // create the subscriber action to the db
                await _repositoryWrapper.SubscriberActionRepository.CreateSubscriberAction(
                     new SubscriberAction()
                     {
                         SubscriberActionGuid = Guid.NewGuid(),
                         CreateDate = DateTime.UtcNow,
                         CreateGuid = Guid.Empty,
                         ActionId = action.ActionId,
                         EntityId = entityId,
                         EntityTypeId = entityType == null ? null : (int?)entityType.EntityTypeId,
                         IsDeleted = 0,
                         OccurredDate = DateTime.UtcNow,
                         SubscriberId = subscriber.SubscriberId
                     });

                // mark as successful if we got to this point
                _syslog.LogTrace($"ScheduledJobs.TrackSubscriberActionInformation success for Subscriber Guid={subscriberGuid}, EntityTypeGuid={entityTypeGuid} and EntityGuid={entityGuid}");
            }
            catch (Exception e)
            {
                // write to syslog
                _syslog.LogError(e, $"ScheduledJobs.TrackSubscriberActionInformation exception: {e.Message} for Subscriber Guid={subscriberGuid}, EntityTypeGuid={entityTypeGuid} and EntityGuid={entityGuid}");
            }
        }

        #endregion

        #region Update MimeType For SubscriberFiles
        /// <summary>
        /// Job to update MimeType for all valid Subscriber Files.
        /// </summary>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task UpdateSubscriberFilesMimeType()
        {
            //get all SubscriberFiles with Empty MimeType
            var queryableSubscriberFile = _repositoryWrapper.SubscriberFileRepository.GetAllSubscriberFileQueryableAsync();
            var nullMimeTypeFiles = await queryableSubscriberFile.Where(x => x.MimeType == null && x.IsDeleted == 0).ToListAsync();

            if (nullMimeTypeFiles.Count > 0)
            {
                foreach (SubscriberFile file in nullMimeTypeFiles)
                {
                    file.MimeType = _mimeMappingService.MapAsync(file.BlobName);
                    await _repositoryWrapper.SubscriberFileRepository.UpdateSubscriberFileAsync(file);
                }
            }
        }
        #endregion

        #region Update for Allegis Group Jobs Raw Data Fix
        /// <summary>
        /// Fix the RawData field in JobPage table for Allegis Group so that when job mining runs, it doesn't classify existing job as new jobs  
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAllegisGroupJobPageRawDataField()
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:UpdateAllegisGroupJobPageRawDataField started at: {DateTime.UtcNow.ToLongDateString()}");
                var jobs = _repositoryWrapper.JobPage.GetAll();
                var allegisjobs = jobs.Where(x => x.JobSiteId == 3 && x.IsDeleted == 0 && x.JobPageStatusId == 2).ToList();
                foreach (var job in allegisjobs)
                {
                    var rawdata = job.RawData;
                    var html = new HtmlDocument();
                    html.LoadHtml(rawdata);
                    var script = html.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");
                    var data = JsonConvert.DeserializeObject<dynamic>(script.InnerText);
                    job.RawData = data.ToString();
                    _repositoryWrapper.JobPage.Update(job);
                }
                await _repositoryWrapper.JobPage.SaveAsync();
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.UpdateAllegisGroupJobPageRawDataField encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
        }

        public async Task SyncAuth0UserId(Guid subscriberGuid, string auth0UserId)
        {
            var currentUtcDateTime = DateTime.UtcNow;
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            if (subscriber != null)
            {
                subscriber.Auth0UserId = auth0UserId;
                subscriber.ModifyDate = currentUtcDateTime;
                subscriber.ModifyGuid = Guid.Empty;
                await _repositoryWrapper.SubscriberRepository.SaveAsync();
            }
        }

        public async Task TrackSubscriberSignIn(Guid subscriberGuid)
        {
            var currentUtcDateTime = DateTime.UtcNow;
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            if (subscriber != null)
            {
                subscriber.LastSignIn = currentUtcDateTime;
                subscriber.ModifyDate = currentUtcDateTime;
                subscriber.ModifyGuid = Guid.Empty;
                await _repositoryWrapper.SubscriberRepository.SaveAsync();
            }
        }

        #endregion

        #region SendGrid jobs

        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public async Task PurgeSendGridAuditRecords()
        {
            try
            {
                int PurgeLookBackDays = int.Parse(_configuration["CareerCircle:SendGridAuditPurgeLookBackDays"]);
                await _repositoryWrapper.StoredProcedureRepository.PurgeSendGridEvents(PurgeLookBackDays);
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.PurgeSendGridAuditRecords encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
        }

        #endregion


        #region HiringSolved Resume Parsing 
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public async Task GetHiringSolvedResumeParseUpdates()
        {
            try
            {
                int BatchSize = int.Parse(_configuration["HiringSolved:UpdateBatchSize"]);
                var Batch = await _repositoryWrapper.HiringSolvedResumeParseRepository.GetAll()
                    .Where(p => p.IsDeleted == 0 && p.ParseStatus == Constants.HiringSolvedStatus.Created)
                    .Take(BatchSize)
                    .ToListAsync();

                foreach (HiringSolvedResumeParse p in Batch)
                    await _hiringSolvedService.GetParseStatus(p.JobId);

                //await _repositoryWrapper.StoredProcedureRepository.PurgeSendGridEvents(PurgeLookBackDays);
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"**** ScheduledJobs.GetHiringSolvedResumeParseUpdates encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
        }



        #endregion



    }
}
