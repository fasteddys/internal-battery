using AutoMapper;
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
            Console.WriteLine("***** WorkFlowMonitor Doing Work: "  + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() );
            return true;
        }

        public Boolean ReconcileFutureEnrollments()
        {
            int MaxReconcilesToProcess = 10;
            int.TryParse(_configuration["Woz:MaxReconcilesToProcess"], out MaxReconcilesToProcess);

            IList<Enrollment> Enrollments = _db.Enrollment
                      .Where(t => t.IsDeleted == 0 && t.EnrollmentStatusId == (int) EnrollmentStatus.FutureRegisterStudentComplete)
                     .ToList<Enrollment>();

            WozInterface wi = new WozInterface(_db, _mapper, _configuration, _syslog);            
            foreach ( Enrollment e in Enrollments)
            {
                wi.ReconcileFutureEnrollment( e.EnrollmentGuid.ToString() );
                if (--MaxReconcilesToProcess == 0)
                    break;                
            }

            Console.WriteLine("***** ReconcileFutureEnrollments Doing Work: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());          
            return true;
        }


    }
}
