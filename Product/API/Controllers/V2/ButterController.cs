using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using System.Security.Claims;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Shared.GoogleJobs;
using System.Collections.Generic;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/butter/")]
    public class ButterController : ControllerBase
    {
 
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IServiceProvider _services;
        private readonly IButterCMSService _butterCMSService;

        #region constructor 
        public ButterController(IServiceProvider services 
        , IJobAlertService jobAlertService
        , IJobFavoriteService jobFavoriteService
        , IJobSearchService jobSearchService
        , ICloudTalentService cloudTalentService
        , ITrackingService trackingService
        , IKeywordService keywordService
        , IButterCMSService butterCMSService    )

        {
            _services = services; 
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _butterCMSService = butterCMSService;
        }
        #endregion

 
        [HttpPut]
        [Route("content-field")]
        public async Task<IActionResult> GetContentField( [FromBody] ButterCMSRequestDto request)
        {
            var rVal = await _butterCMSService.RetrieveContentFieldsAsync<dynamic>(request.Keys, request.QueryParameters);
            return Ok(rVal);
        }


        [HttpPut]
        [Route("page")]
        public async Task<IActionResult> GetPage([FromBody] ButterCMSRequestDto request)
        {
            var rVal = await _butterCMSService.RetrievePageAsync<dynamic>(request.Url, request.QueryParameters);
            return Ok(rVal);
        }


        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("cache/page/clear/{slug}")]
        public async Task<IActionResult> ClearPageCache(string slug)
        {
            var rVal = await _butterCMSService.ClearCachedPageAsync(slug);
            return Ok();
        }

        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("cache/clear/{key}")]
        public async Task<IActionResult> ClearCache(string key)
        {
            var rVal = await _butterCMSService.ClearCachedKeyAsync(key);
            return Ok();
        }




    }




}
