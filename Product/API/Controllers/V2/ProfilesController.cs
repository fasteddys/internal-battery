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
using UpDiddyLib.Dto.User;
using UpDiddyLib.Domain;


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
        private readonly CloudTalentService _cloudTalentService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly IJobService _jobService;
        private readonly IHangfireService _hangfireService;
        private readonly ISubscriberService _subscriberService;
        private readonly ISubscriberEducationalHistoryService _subscriberEducationalHistoryService;
        private readonly ISubscriberWorkHistoryService _subscriberWorkHistoryService;
        private readonly IJobPostingService _jobPostingService;
        private readonly IJobAlertService _jobAlertService;
        private readonly IProfileService _profileService;


        #region constructor 
        public ProfilesController(IServiceProvider services, IHangfireService hangfireService, CloudTalentService cloudTalentService)


        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _subscriberWorkHistoryService = _services.GetService<ISubscriberWorkHistoryService>();
            _subscriberEducationalHistoryService = _services.GetService<ISubscriberEducationalHistoryService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalentService = cloudTalentService;
            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
            _profileService = _services.GetService<IProfileService>();
        }

        #endregion



        #region social profile 


        [HttpPut]
        [Authorize]
        [Route("/V2/[controller]/socials")]
        public async Task<IActionResult> UpdateSocialProfile([FromBody] SubscriberProfileSocialDto subscribeProfileSocialDto)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _profileService.UpdateSubscriberProfileSocialAsync(subscribeProfileSocialDto, subscriberGuid);
            return StatusCode(210);
        }


        [HttpGet]
        [Authorize]
        [Route("/V2/[controller]/socials")]
        public async Task<SubscriberProfileSocialDto> GetSocialProfile()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return ( await _profileService.GetSubscriberProfileSocialAsync(subscriberGuid) );
            
        }




        #endregion



        #region basic profile

        [HttpPost]
        [Authorize]
        [Route("/V2/[controller]")]
        public async Task<IActionResult> AddProfile([FromBody] SubscribeProfileBasicDto subscribeProfileBasicDto )
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _profileService.CreateNewSubscriberAsync(subscribeProfileBasicDto); 
            return StatusCode(201);
        }


        [HttpPut]
        [Authorize]
        [Route("/V2/[controller]")]
        public async Task<IActionResult> UpdateProfile([FromBody] SubscribeProfileBasicDto subscribeProfileBasicDto)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _profileService.UpdateSubscriberProfileBasicAsync(subscribeProfileBasicDto, subscriberGuid);
            return StatusCode(200);
        }


        [HttpGet]
        [Authorize]
        [Route("/V2/[controller]/{subscriberGuid}")]
        public async Task<SubscribeProfileBasicDto> GetProfile(Guid subscriberGuid)
        {
            return ( await _profileService.GetSubscriberProfileBasicAsync(subscriberGuid));            
        }



        #endregion


        #region educational history

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
        public async Task<IActionResult> GetProfileEducationHistory()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var rVal = await _subscriberEducationalHistoryService.GetEducationalHistory(subscriberGuid);
            return Ok(rVal);
        }

        #endregion

        #region work history

        [HttpPost]
        [Authorize]
        [Route("/V2/[controller]/work-histories")]
        public async Task<IActionResult> AddWorkHistory([FromBody] SubscriberWorkHistoryDto subscriberEducationHistoryDto)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _subscriberWorkHistoryService.AddWorkHistory(subscriberEducationHistoryDto, subscriberGuid);
            return StatusCode(201);
        }

        [HttpPut]
        [Authorize]
        [Route("/V2/[controller]/work-histories/{workHistoryGuid}")]
        public async Task<IActionResult> UpdateWorkHistory([FromBody] SubscriberWorkHistoryDto subscriberEducationHistoryDto, Guid workHistoryGuid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _subscriberWorkHistoryService.UpdateEducationalHistory(subscriberEducationHistoryDto, subscriberGuid, workHistoryGuid);
            return StatusCode(200);
        }


        [HttpDelete]
        [Authorize]
        [Route("/V2/[controller]/work-histories/{workHistoryGuid}")]
        public async Task<IActionResult> DeleteWorkHistory(Guid workHistoryGuid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _subscriberWorkHistoryService.DeleteWorklHistory(subscriberGuid, workHistoryGuid);
            return StatusCode(204);
        }


        [HttpGet]
        [Authorize]
        [Route("/V2/[controller]/work-histories")]
        public async Task<IActionResult> GetWorkHistory()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var rVal = await _subscriberWorkHistoryService.GetWorkHistory(subscriberGuid);
            return Ok(rVal);
        }


        #endregion


    }
} 