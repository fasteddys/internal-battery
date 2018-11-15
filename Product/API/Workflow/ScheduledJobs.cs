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
            _HttpClientFactory = httpClientFactory;
        }


        #region Woz


        public bool UpdateWozStudentLastLogin(string SubscriberGuid )
        {
            try
            {
                _syslog.Log(LogLevel.Information,$"***** UpdateWozStudentLastLogin started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid.ToString()}");
                Subscriber subscriber = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == Guid.Parse(SubscriberGuid))
                .FirstOrDefault();

                if (subscriber == null)
                    return false;

                int WozVendorId = int.Parse(_configuration["Woz:VendorId"]);
                VendorStudentLogin studentLogin = _db.VendorStudentLogin
                    .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriber.SubscriberId && s.VendorId == WozVendorId) 
                    .FirstOrDefault();

                if (studentLogin == null)
                    return false;

                DateTime? LastLoginDate = GetWozStudentLastLogin(int.Parse(studentLogin.VendorLogin));

                    _db.SaveChanges();

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
                _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid.ToString()}");
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
                    // Only Call woz if the modify date is null or if the modify date older that progress update age threshold
                    if (e.ModifyDate == null || ((DateTime)e.ModifyDate).AddHours(ProgressUpdateAgeThresholdInHours) <= DateTime.Now)
                    {
                        wcp = GetWozCourseProgress(e);
                        if (wcp != null && wcp.ActivitiesCompleted > 0 && wcp.ActivitiesTotal > 0)
                        {
                            updatesMade = true;
                            e.PercentComplete = Convert.ToInt32((wcp.ActivitiesCompleted / wcp.ActivitiesTotal) * 100);
                            e.ModifyDate = DateTime.Now;
                        }
                    }
                }
                if (updatesMade)
                    _db.SaveChanges();

                _syslog.Log(LogLevel.Information, $"***** UpdateStudentProgress completed at: {DateTime.UtcNow.ToLongDateString()}");
                return true;
            }
            catch ( Exception e )
            {
                _syslog.Log(LogLevel.Error, "UpdateStudentProgress:GetWozCourseProgress threw an exception -> " + e.Message);
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

                WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog,_HttpClientFactory);
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
                WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog, _HttpClientFactory);
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
                WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog, _HttpClientFactory);
                WozStudentInfoDto studentLogin = wi.GetStudentLastLogin(exeterId).Result;
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
