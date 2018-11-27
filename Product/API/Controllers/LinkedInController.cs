using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Newtonsoft;
using System.Reflection;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UpDiddyLib.Helpers;
using AutoMapper.QueryableExtensions;
using UpDiddyApi.Workflow;
using Hangfire;
using UpDiddy.Helpers;
using UpDiddyApi.Business;

namespace UpDiddyApi.Controllers
{
    public class LinkedInController : Controller
    {
        #region Class Members
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly ISysLog _syslog;
        private readonly ISysEmail _sysemail;
        private readonly IServiceProvider _serviceProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly string _apiBaseUri = String.Empty;
        private WozTransactionLog _log = null;
        private IHttpClientFactory _httpClientFactory = null;
        #endregion

        #region Constructor
        public LinkedInController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _apiBaseUri = _configuration["LinkedIn:ApiUrl"]; 
            _log = new WozTransactionLog();
            _syslog = new SysLog(configuration, sysemail, serviceProvider);
            _httpClientFactory = httpClientFactory;
            _sysemail = sysemail;
            _serviceProvider = serviceProvider;
        }
        #endregion
 


        // TODO AUTHORIZE
        [HttpGet]
        [Route("api/[controller]/SyncProfile/{SubscriberGuid}/{Code}")]
        public IActionResult SyncProfile(Guid SubscriberGuid, string Code)
        {
            // Enqueue job and return 
            BackgroundJob.Enqueue<LinkedInInterface>(lif => lif.SyncProfile(SubscriberGuid, Code) );
            BasicResponseDto rVal = new BasicResponseDto()
            {
                StatusCode = ((int) ProfileDataStatus.Processing).ToString(),
                Description = ProfileDataStatus.Processing.ToString()
            };
            return Ok(rVal);            
        }



        // TODO AUTHORIZE
        [HttpGet]
        [Route("api/[controller]/LastSyncDate/{SubscriberGuid}")]
        public IActionResult LastSyncDate(Guid SubscriberGuid)
        {

            ValueResponseDto rVal = new ValueResponseDto();

            LinkedInToken lit = _db.LinkedInToken
            .Where(t => t.IsDeleted == 0 && t.SubscriberGuid == SubscriberGuid)
            .FirstOrDefault();

            if (lit != null)
            {
                rVal.ValueDateTime = lit.ModifyDate;
                rVal.StatusCode = (int)ProfileDataStatus.Acquired;
                rVal.ValueString = ProfileDataStatus.Acquired.ToString();
            }
            else
            {
                rVal.ValueDateTime = null ;
                rVal.StatusCode = (int)ProfileDataStatus.AccountNotFound;
                rVal.ValueString = ProfileDataStatus.AccountNotFound.ToString();
            }

            return Ok(rVal);
        }


    }
}