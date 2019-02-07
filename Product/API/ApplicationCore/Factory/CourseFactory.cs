using Hangfire;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Logging;

namespace UpDiddyApi.ApplicationCore
{
    public class CourseFactory : FactoryBase
    {
        private readonly UpDiddyDbContext _db = null;

        #region constructor
        public CourseFactory(UpDiddyDbContext db, IConfiguration configuration, ILogger syslog, IDistributedCache distributedCache) :
            base(db, configuration, syslog, distributedCache)
        {
            _db = db;
        }
        #endregion

        #region public factory methods 
        public CourseLoginDto GetCourseLogin(Guid SubscriberGuid, Guid EnrollmentGuid)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** CourseFactory.GetCourseLogin started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid} for enrollment {EnrollmentGuid}");
                CourseLoginDto rVal = new CourseLoginDto();

                var query = _db.Course
                    .Include(c => c.Vendor)
                    .Where(c => c.IsDeleted == 0)
                    .Join(_db.Enrollment, c => c.CourseId, e => e.CourseId, (c, e) => new { Course = c, Enrollment = e })
                    .Where(x => x.Enrollment.EnrollmentGuid.Value == EnrollmentGuid)
                    .FirstOrDefault();

                var courseGuid = query.Course.CourseGuid.Value;
                var vendorGuid = query.Course.Vendor.VendorGuid.Value;


                rVal.LoginUrl = string.Empty;
                if (vendorGuid.ToString() == _configuration["Woz:VendorGuid"])
                    rVal = GetWozCourseLogin(SubscriberGuid, courseGuid, vendorGuid);
                else
                    rVal.LoginUrl = "Unknown Vendor";

                _syslog.Log(LogLevel.Information, $"***** CourseFactory.GetCourseLogin completed at: {DateTime.UtcNow.ToLongDateString()}");
                return rVal;
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Error, "CourseFactory.GetCourseLogin threw an exception -> " + ex.Message);
                _syslog.Log(LogLevel.Error, $"Parameters subscriber= {SubscriberGuid}  enrollment= {EnrollmentGuid}");
                return new CourseLoginDto()
                {
                    LoginUrl = "Error in CourseFactory.GetCourseLogin"
                };
            }
        }


        public static Course GetCourseById(UpDiddyDbContext db, int courseId)
        {
            return db.Course
                .Where(s => s.IsDeleted == 0 && s.CourseId == courseId)
                .FirstOrDefault();
        }


        #endregion

        #region Vendor specific methods
        private CourseLoginDto GetWozCourseLogin(Guid SubscriberGuid, Guid CourseGuid, Guid VendorGuid)
        {
            return _GetWozCourseLogin(SubscriberGuid, CourseGuid, VendorGuid);
        }

        #endregion

        #region Helper methods 
        private CourseLoginDto _GetWozCourseLogin(Guid SubscriberGuid, Guid CourseGuid, Guid VendorGuid)
        {
            int step = 0;
            try
            {

                _syslog.Log(LogLevel.Information, $"***** CourseFactory.GetWozCourseLogin started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid} for course {CourseGuid} and vendor {VendorGuid}");
                CourseLoginDto rVal = new CourseLoginDto();
                rVal.LoginUrl = string.Empty;

                var studentLogin =
                            from vsl in _db.VendorStudentLogin
                            join v in _db.Vendor on vsl.VendorId equals v.VendorId
                            join s in _db.Subscriber on vsl.SubscriberId equals s.SubscriberId
                            where s.SubscriberGuid == SubscriberGuid && vsl.VendorId == v.VendorId
                            select new { LastLoginDate = vsl.LastLoginDate, SubscriberGuid = s.SubscriberGuid, RegstrationUrl = vsl.RegistrationUrl, LoginUrl = v.LoginUrl };

                step = 1;
                if (studentLogin != null && studentLogin.Any())
                {
                    DateTime? LastLoginDate = studentLogin.First().LastLoginDate;
                    Guid? subscriberGuid = studentLogin.First().SubscriberGuid;
                    step = 2;
                    if (LastLoginDate == null)
                    {
                        step = 3;
                        int HangFireDelay = int.Parse(_configuration["Woz:LastLoginUpdateDelayInMinutes"]);
                        step = 4;
                        // Call hangfire to check to see if the user has logged into woz 
                        if (subscriberGuid != null)
                        {
                            step = 5;
                            string cacheKey = $"CourseFactoryGetCourseLogin{subscriberGuid}";
                            string inProgressFlag = _cache.GetString(cacheKey);
                            // Only queue hangfire job is it has not been done recently
                            step = 6;
                            if (string.IsNullOrEmpty(inProgressFlag))
                            {
                                step = 7;
                                BackgroundJob.Schedule<ScheduledJobs>(j => j.UpdateWozStudentLastLogin(subscriberGuid.ToString()), TimeSpan.FromMinutes(HangFireDelay));
                                // The hangfire delay should be sufficient for the the cache TTL
                                _cache.SetString(cacheKey, "Inprogress", new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(HangFireDelay) });
                            }
                        }
                        step = 8;
                        rVal.LoginUrl = studentLogin.First().RegstrationUrl;
                    }
                    else
                    {
                        step = 9;
                        rVal.LoginUrl = studentLogin.First().LoginUrl;
                    }

                }
                _syslog.Log(LogLevel.Information, $"***** CourseFactory.GetWozCourseLogin completed at: {DateTime.UtcNow.ToLongDateString()}");
                step = 10;
                return rVal;
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Error, $"CourseFactory.GetWozCourseLogin: Exception at step {step} -> {ex.Message}");
                _syslog.Log(LogLevel.Error, $"CourseFactory.GetWozCourseLogin: Parameters subscriber= {SubscriberGuid}  course= {CourseGuid} vendor= {VendorGuid}");
                return new CourseLoginDto()
                {
                    LoginUrl = "Error in CourseFactory.GetWozCourseLogin"
                };
            }
        }
        #endregion
    }
}

