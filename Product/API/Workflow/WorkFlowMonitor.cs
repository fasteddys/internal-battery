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
            // TODO standardize configuration nameing -> Woz:ApiUrl
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



    }
}
