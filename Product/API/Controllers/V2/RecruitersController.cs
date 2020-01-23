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
    [Route("/V2/[controller]/")]
    [ApiController]
    public class RecruitersController : BaseApiController
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;        
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly IJobService _jobService;
        private readonly IRecruiterService _recruiterService;


        #region constructor 
        public RecruitersController(IServiceProvider services
        , IHangfireService hangfireService
        , IRecruiterService recruiterService
        , IJobFavoriteService jobFavoriteService
        , IJobSearchService jobSearchService
        , ICloudTalentService cloudTalentService
        , ITrackingService trackingService
        , IKeywordService keywordService)

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();            
            _recruiterService = recruiterService;            
        }

        #endregion



        // TODO JAB Add migration for sproc 

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> GetJobAdminForSubscriber(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var rVal = await  _recruiterService.GetRecruiters(limit, offset, sort, order); 
            return Ok(rVal);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateRecruiter( [FromBody] RecruiterInfoDto recruiterInfoDto)
        {

            await _recruiterService.AddRecruiterAsync(recruiterInfoDto);
            return StatusCode(201);
        }


        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateRecruiter([FromBody] RecruiterInfoDto recruiterInfoDto)
        {

            await _recruiterService.EditRecruiterAsync(recruiterInfoDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("{RecruiterGuid}")]
        public async Task<IActionResult> DeleteRecruiter( Guid RecruiterGuid)
        {

            await _recruiterService.DeleteRecruiterAsync(GetSubscriberGuid(), RecruiterGuid);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("{RecruiterGuid}")]
        public async Task<IActionResult> GetRecruiter(Guid RecruiterGuid)
        {

            RecruiterInfoDto rVal = await _recruiterService.GetRecruiter(RecruiterGuid);
            return Ok(rVal);
        }


    }
}