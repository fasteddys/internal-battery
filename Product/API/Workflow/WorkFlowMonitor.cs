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

namespace UpDiddyApi.Workflow
{
    public class WorkFlowMonitor : BusinessVendorBase
    {

        public WorkFlowMonitor(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysLog sysLog)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["WozApiUrl"];
            _accessToken = configuration["WozAccessToken"];
            _syslog = sysLog;
            _configuration = configuration;
        }


        public Boolean DoWork()
        {
            Console.WriteLine("***** WorkFlowMonitor Doing Work: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            return true;
        }

        public Boolean ReconcileFutureEnrollments()
        {
            int MaxReconcilesToProcess = 10;
            int.TryParse(_configuration["Woz:MaxReconcilesToProcess"], out MaxReconcilesToProcess);

            IList<Enrollment> Enrollments = _db.Enrollment
                      .Where(t => t.IsDeleted == 0 && t.EnrollmentStatusId == (int)EnrollmentStatus.FutureRegisterStudentComplete)
                     .ToList<Enrollment>();

            WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog);
            foreach (Enrollment e in Enrollments)
            {
                wi.ReconcileFutureEnrollment(e.EnrollmentGuid.ToString());
                if (--MaxReconcilesToProcess == 0)
                    break;
            }

            Console.WriteLine("***** ReconcileFutureEnrollments Doing Work: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            return true;
        }

        public Boolean DoPromoCodeRedemptionCleanup(int? lookbackPeriodInMinutes = 30)
        {
            bool result = false;
            try
            {
                Console.WriteLine($"***** DoPromoCodeRedemptionCleanup started at: {DateTime.UtcNow.ToLongDateString()}");

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
                // SysLog? 
                throw e;
            }
            finally
            {
                Console.WriteLine($"***** DoPromoCodeRedemptionCleanup completed at: {DateTime.UtcNow.ToLongDateString()}");

            }
            return result;
        }
    }
}
