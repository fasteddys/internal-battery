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

namespace UpDiddyApi.Workflow
{
    public class ScheduledJobs : BusinessVendorBase 
    {

        public ScheduledJobs(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration,ISysEmail sysEmail, IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["Woz:ApiUrl"];
            _accessToken = configuration["Woz:AccessToken"];
            _syslog = new SysLog(configuration, sysEmail, serviceProvider);
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

                WozCourseProgress wcp = null;
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

        private WozCourseProgress GetWozCourseProgress(Enrollment enrollment)
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

                WozCourseProgress Wcp = wi.GetCourseProgress(wce.SectionId, wce.WozEnrollmentId).Result;
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
        public Boolean DoPromoCodeRedemptionCleanup(int? lookbackPeriodInMinutes = 30)
        {
            bool result = false;
            try
            {
                _syslog.Log(LogLevel.Information, $"***** DoPromoCodeRedemptionCleanup started at: {DateTime.UtcNow.ToLongDateString()}");

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

                //.ForEachAsync(abandonedPromoCodeRedemption =>
                //{
                //    abandonedPromoCodeRedemption.ModifyDate = DateTime.UtcNow;
                //    abandonedPromoCodeRedemption.ModifyGuid = Guid.NewGuid();
                //    abandonedPromoCodeRedemption.IsDeleted = 1;
                //    _db.Attach<PromoCodeRedemption>(abandonedPromoCodeRedemption);
                //});

                _db.SaveChanges();

                result = true;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error,"ScheduledJobs:DoPromoCodeRedemptionCleanup threw an exception -> " + e.Message);
                throw e;
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** DoPromoCodeRedemptionCleanup completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
            return result;
        }

        #endregion


    }
}
