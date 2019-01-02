using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Business;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyLib.Dto;
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;
using Hangfire;
using System.Net.Http;
using UpDiddy.Helpers;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Business.Resume;

namespace UpDiddyApi.Workflow
{
    public class ScheduledJobs : BusinessVendorBase 
    {

        public ScheduledJobs(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration,ISysEmail sysEmail, IHttpClientFactory httpClientFactory, ILogger<ScheduledJobs> logger, ISovrenAPI sovrenApi)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["Woz:ApiUrl"];
            _accessToken = configuration["Woz:AccessToken"];
            _syslog = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _sovrenApi = sovrenApi;
        }


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
                    .Where(v => v.IsDeleted == 0 && v.Name ==  Constants.WozVendorName)
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
                if (LastLoginDate != null &&  (studentLogin.LastLoginDate == null || LastLoginDate > studentLogin.LastLoginDate))
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
                    if (e.ModifyDate == null || ((DateTime)e.ModifyDate).AddHours(ProgressUpdateAgeThresholdInHours) <= DateTime.Now)
                    {
                        _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress calling woz for enrollment {e.EnrollmentGuid}");
                        wcp = GetWozCourseProgress(e);
                        if (wcp != null && wcp.ActivitiesCompleted > 0 && wcp.ActivitiesTotal > 0)
                        {
                            _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress updating enrollment {e.EnrollmentGuid}");
                            updatesMade = true;
                            e.PercentComplete = Convert.ToInt32(((double) wcp.ActivitiesCompleted / (double) wcp.ActivitiesTotal) * 100);
                            _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress updating enrollment {e.EnrollmentGuid} set PercentComplete={e.PercentComplete}");
                            e.ModifyDate = DateTime.Now;
                        }
                        else
                        {
                            if ( wcp == null  )
                                _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress GetWozCourseProgress returned null for enrollment {e.EnrollmentGuid}");
                            else
                                _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress GetWozCourseProgress returned ActivitiesCompleted = {wcp.ActivitiesCompleted} ActivitiesTotal = {wcp.ActivitiesTotal}");
                        }
                    }
                    else
                    {
                        DateTime ModifyDate = (DateTime) e.ModifyDate;
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
            catch ( Exception e )
            {
                _syslog.Log(LogLevel.Error, $"UpdateStudentProgress:GetWozCourseProgress threw an exception -> {e.Message} for subscriber {SubscriberGuid}" );
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

                WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog,_httpClientFactory);
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
                _syslog.Log(LogLevel.Error,"ScheduledJobs:ReconcileFutureEnrollments threw an exception -> " + e.Message);                
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


        #region CareerCircle Jobs 
 
        public Boolean ImportSubscriberProfileData(ResumeDto resumeDto,Subscriber subscriber)
        {
            try
            {
                string errMsg = string.Empty;

                _syslog.Log(LogLevel.Information, $"***** ScheduledJobs:ImportSubscriberProfileData started at: {DateTime.UtcNow.ToLongDateString()} subscriberGuid = {resumeDto.SubscriberGuid}");

                String parsedDocument =  _sovrenApi.SubmitResumeAsync(resumeDto.Base64EncodedResume).Result;
                SubscriberProfileStagingStore.Save(_db, subscriber, Constants.DataSource.Sovren, Constants.DataFormat.Xml, parsedDocument);

                // Get the list of profiles that need 
                List<SubscriberProfileStagingStore> profiles = _db.SubscriberProfileStagingStore
                .Include(p => p.Subscriber)
                .Where(p => p.IsDeleted == 0 && p.Status == (int)ProfileDataStatus.Acquired && p.Subscriber.SubscriberGuid == resumeDto.SubscriberGuid)
                .ToList();

                _ImportSubscriberProfileData(profiles);
            }
            catch (Exception e)
            {
                // Save any work that has been completed before the exception 
                _db.SaveChanges();
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
                // TODO remove debug code 
                Process process = Process.GetCurrentProcess();
                var WorkingSet64 = process.WorkingSet64; ;
                var Threads = process.Threads.Count;
                var VirtualMemorySize64 = process.VirtualMemorySize64;

                _syslog.Log(LogLevel.Information, $"SystemHealth WorkingSet64: {WorkingSet64.ToString()}");
                _syslog.Log(LogLevel.Information, $"SystemHealth Thread Count: {Threads}");
                _syslog.Log(LogLevel.Information, $"SystemHealth VirtualMemorySize64: {VirtualMemorySize64}");

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
                        abandonedPromoCodeRedemption.ModifyGuid = Guid.NewGuid();
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
                        p.Status = (int)SubscriberSkill.ImportLinkedIn(_db, _sovrenApi, p, ref errMsg);
                    else if (p.ProfileSource == Constants.DataSource.Sovren)                    
                        p.Status = (int)SubscriberSkill.ImportSovren(_db, p, ref errMsg);                    
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



    }
}
