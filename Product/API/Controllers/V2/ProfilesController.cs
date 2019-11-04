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
using UpDiddyLib.Dto.Marketing;

namespace UpDiddyApi.Controllers.V2
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;
        private readonly CloudTalent _cloudTalent = null;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly IJobService _jobService;
        private readonly IHangfireService _hangfireService;
        private readonly ISubscriberService _subscriberService;
        private readonly ISubscriberEducationalHistoryService _subscriberEducationalHistoryService;
        private readonly IJobPostingService _jobPostingService;
        private readonly IJobAlertService _jobAlertService;


        #region constructor 
        public ProfilesController(IServiceProvider services, IHangfireService hangfireService)


        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _subscriberEducationalHistoryService = _services.GetService<ISubscriberEducationalHistoryService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
        }

        #endregion


 



        [HttpPost]
        [Authorize]
        [Route("/V2/[controller]/education-histories")]
        public async Task<IActionResult> AddProfileEducationHistory([FromBody] SubscriberEducationHistoryDto subscriberEducationHistoryDto)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _subscriberEducationalHistoryService.CreateEducationalHistory(subscriberEducationHistoryDto,subscriberGuid);
            return StatusCode(201);
        }


        [HttpPut]
        [Authorize]
        [Route("/V2/[controller]/education-histories/{educationalHistoryGuid}")]
        public async Task<IActionResult> UpdateProfileEducationHistory([FromBody] SubscriberEducationHistoryDto subscriberEducationHistoryDto, Guid educationalHistoryGuid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _subscriberEducationalHistoryService.UpdateEducationalHistory(subscriberEducationHistoryDto, subscriberGuid, educationalHistoryGuid);
            return StatusCode(200);
        }


        [HttpDelete]
        [Authorize]
        [Route("/V2/[controller]/education-histories/{educationalHistoryGuid}")]
        public async Task<IActionResult> DeleteProfileEducationHistory(Guid educationalHistoryGuid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _subscriberEducationalHistoryService.DeleteEducationalHistory(subscriberGuid, educationalHistoryGuid);
            return StatusCode(200);
        }



        [HttpGet]
        [Authorize]
        [Route("/V2/[controller]/education-histories")]
        public async Task<IActionResult> GetProfileEducationHistory(Guid educationalHistoryGuid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var rVal = await _subscriberEducationalHistoryService.GetEducationalHistory(subscriberGuid);
            return Ok(rVal);
        }




    }
} 