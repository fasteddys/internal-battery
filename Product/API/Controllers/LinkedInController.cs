using System;
using System.Linq;
using System.Net.Http;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using Hangfire;
using UpDiddyApi.Business;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.Controllers
{
    public class LinkedInController : Controller
    {
        #region Class Members
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly ISysLog _syslog;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly string _apiBaseUri = String.Empty;
        private WozTransactionLog _log = null;
        private IHttpClientFactory _httpClientFactory = null;
        #endregion

        #region Constructor
        public LinkedInController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration,  IHttpClientFactory httpClientFactory, ISysLog sysLog)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _apiBaseUri = _configuration["LinkedIn:ApiUrl"]; 
            _log = new WozTransactionLog();
            //_syslog = new SysLog(configuration, sysemail, serviceProvider);
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;            
        }
        #endregion





       // TODO  [Authorize]
        [HttpGet]
        [Route("api/[controller]/GetProfile/{SubscriberGuid}")]
        public IActionResult GetProfile(Guid subscriberGuid )
        {
            var rVal = SubscriberProfileStagingStore.GetProfileAsLinkedInDto(_db, subscriberGuid);
            return Ok(rVal); ;
        }



        [Authorize]
        [HttpGet]
        [Route("api/[controller]/SyncProfile/{SubscriberGuid}/{Code}")]
        public IActionResult SyncProfile(Guid subscriberGuid, string code )
        {

            var returnUrl = Request.Query["returnUrl"].ToString();
            // Enqueue job and return 
            BackgroundJob.Enqueue<LinkedInInterface>(lif => lif.SyncProfile(subscriberGuid, code, returnUrl) );
            BasicResponseDto rVal = new BasicResponseDto()
            {
                StatusCode = ((int) ProfileDataStatus.Processing).ToString(),
                Description = ProfileDataStatus.Processing.ToString()
            };
            return Ok(rVal);            
        }



        [Authorize]
        [HttpGet]
        [Route("api/[controller]/LastSyncDate/{SubscriberGuid}")]
        public IActionResult LastSyncDate(Guid subscriberGuid)
        {

            ValueResponseDto rVal = new ValueResponseDto();

            LinkedInToken lit = _db.LinkedInToken
                    .Include(l => l.Subscriber)
                    .Where(l => l.IsDeleted == 0 && l.Subscriber.SubscriberGuid == subscriberGuid)
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