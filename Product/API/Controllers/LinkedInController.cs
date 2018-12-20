﻿using System;
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
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UpDiddyApi.Controllers
{
    public class LinkedInController : Controller
    {
        #region Class Members
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly ILogger _syslog;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly string _apiBaseUri = String.Empty;
        private WozTransactionLog _log = null;
        private IHttpClientFactory _httpClientFactory = null;
        #endregion

        #region Constructor
        public LinkedInController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration,  IHttpClientFactory httpClientFactory, ILogger<LinkedInController> sysLog)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _apiBaseUri = _configuration["LinkedIn:ApiUrl"]; 
            _log = new WozTransactionLog();
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;            
        }
        #endregion

 
        [Authorize]
        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult GetProfile()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var rVal = SubscriberProfileStagingStore.GetProfileAsLinkedInDto(_db, subscriberGuid,_syslog);
            return Ok(rVal); ;
        }



        [Authorize]
        [HttpPut]
        [Route("api/[controller]/SyncProfile/{Code}")]
        public IActionResult SyncProfile(string code)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var returnUrl = Request.Query["returnUrl"].ToString();
            if (String.IsNullOrEmpty(returnUrl))
                return BadRequest(new { code = 400, message = "Missing return url as get param." });

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
        [Route("api/[controller]/LastSyncDate")]
        public IActionResult LastSyncDate()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
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