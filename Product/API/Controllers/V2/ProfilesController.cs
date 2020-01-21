using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
using System.Collections.Generic;
using UpDiddyApi.ApplicationCore.ActionFilter;

namespace UpDiddyApi.Controllers.V2
{
    [ServiceFilter(typeof(ActionFilter))]
    [Route("/V2/[controller]/")]
    public class ProfilesController : BaseApiController
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;
        private readonly ICloudTalentService _cloudTalentService;
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
        private readonly IResumeService _resumeService;
        private readonly ISkillService _skillservice;
        private readonly IAvatarService _avatarService;
        private readonly ISubscriberCourseService _subscriberCourseService;
        private IAuthorizationService _authorizationService;


        #region constructor 
        public ProfilesController(IServiceProvider services, IHangfireService hangfireService, ICloudTalentService cloudTalentService, IResumeService resumeService, ISkillService skillservice, IAvatarService avatarService, IAuthorizationService authorizationService)
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
            _resumeService = resumeService;
            _skillservice = skillservice;
            _avatarService = avatarService;
            _subscriberCourseService = _services.GetService<ISubscriberCourseService>();
            _authorizationService = authorizationService;
        }

        #endregion

        #region social profile 

        [HttpPut]
        [Authorize]
        [Route("socials")]
        public async Task<IActionResult> UpdateSocialProfile([FromBody] SubscriberProfileSocialDto subscribeProfileSocialDto)
        {
            await _profileService.UpdateSubscriberProfileSocialAsync(subscribeProfileSocialDto, GetSubscriberGuid());
            return StatusCode(200);
        }

        [HttpGet]
        [Authorize]
        [Route("socials")]
        public async Task<SubscriberProfileSocialDto> GetSocialProfile()
        {
            return (await _profileService.GetSubscriberProfileSocialAsync(GetSubscriberGuid()));
        }

        #endregion

        #region basic profile

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProfile([FromBody] SubscribeProfileBasicDto subscribeProfileBasicDto)
        {
            await _profileService.CreateNewSubscriberAsync(subscribeProfileBasicDto);
            return StatusCode(201);
        }


        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] SubscribeProfileBasicDto subscribeProfileBasicDto)
        {

            await _profileService.UpdateSubscriberProfileBasicAsync(subscribeProfileBasicDto, GetSubscriberGuid());
            return StatusCode(200);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _profileService.GetSubscriberProfileBasicAsync(GetSubscriberGuid());
            return Ok(profile);
        }

        #endregion

        #region educational history

        [HttpPost]
        [Authorize]
        [Route("education-histories")]
        public async Task<IActionResult> AddProfileEducationHistory([FromBody] SubscriberEducationHistoryDto subscriberEducationHistoryDto)
        {
            var educationGuid = await _subscriberEducationalHistoryService.CreateEducationalHistory(subscriberEducationHistoryDto, GetSubscriberGuid());
            return Ok(educationGuid);
        }

        [HttpPut]
        [Authorize]
        [Route("education-histories/{educationalHistoryGuid:guid}")]
        public async Task<IActionResult> UpdateProfileEducationHistory([FromBody] SubscriberEducationHistoryDto subscriberEducationHistoryDto, Guid educationalHistoryGuid)
        {
            await _subscriberEducationalHistoryService.UpdateEducationalHistory(subscriberEducationHistoryDto, GetSubscriberGuid(), educationalHistoryGuid);
            return StatusCode(200);
        }

        [HttpDelete]
        [Authorize]
        [Route("education-histories/{educationalHistoryGuid:guid}")]
        public async Task<IActionResult> DeleteProfileEducationHistory(Guid educationalHistoryGuid)
        {
            await _subscriberEducationalHistoryService.DeleteEducationalHistory(GetSubscriberGuid(), educationalHistoryGuid);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize]
        [Route("education-histories")]
        public async Task<IActionResult> GetProfileEducationHistory()
        {
            var rVal = await _subscriberEducationalHistoryService.GetEducationalHistory(GetSubscriberGuid());
            return Ok(rVal);
        }

        #endregion

        #region work history

        [HttpPost]
        [Authorize]
        [Route("work-histories")]
        public async Task<IActionResult> AddWorkHistory([FromBody] SubscriberWorkHistoryDto subscriberEducationHistoryDto)
        {
            var workHistoryGuid = await _subscriberWorkHistoryService.AddWorkHistory(subscriberEducationHistoryDto, GetSubscriberGuid());
            return Ok(workHistoryGuid);
        }

        [HttpPut]
        [Authorize]
        [Route("work-histories/{workHistoryGuid:guid}")]
        public async Task<IActionResult> UpdateWorkHistory([FromBody] SubscriberWorkHistoryDto subscriberEducationHistoryDto, Guid workHistoryGuid)
        {
            await _subscriberWorkHistoryService.UpdateEducationalHistory(subscriberEducationHistoryDto, GetSubscriberGuid(), workHistoryGuid);
            return StatusCode(200);
        }

        [HttpDelete]
        [Authorize]
        [Route("work-histories/{workHistoryGuid:guid}")]
        public async Task<IActionResult> DeleteWorkHistory(Guid workHistoryGuid)
        {
            await _subscriberWorkHistoryService.DeleteWorklHistory(GetSubscriberGuid(), workHistoryGuid);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize]
        [Route("work-histories")]
        public async Task<IActionResult> GetWorkHistory()
        {
            var rVal = await _subscriberWorkHistoryService.GetWorkHistory(GetSubscriberGuid());
            return Ok(rVal);
        }

        #endregion

        #region Resume

        [HttpGet]
        [Authorize]
        [Route("resume")]
        public async Task<IActionResult> DownloadResume()
        {
            var resume = await _resumeService.DownloadResume(GetSubscriberGuid());
            return Ok(resume);
        }

        [HttpDelete]
        [Authorize]
        [Route("resume")]
        public async Task<IActionResult> DeleteResume()
        {
            await _resumeService.DeleteResume(GetSubscriberGuid());
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize]
        [Route("resume")]
        public async Task<IActionResult> UploadResume([FromBody] UpDiddyLib.Domain.Models.FileDto fileDto)
        {
            var resumeParseGuid = await _resumeService.UploadResume(GetSubscriberGuid(), fileDto);
            return Ok(resumeParseGuid);
        }

        [HttpGet]
        [Authorize]
        [Route("resume/parse")]
        public async Task<IActionResult> GetResumeParse()
        {
            var resumeParseGuid = await _resumeService.GetResumeParse(GetSubscriberGuid());
            return Ok(resumeParseGuid);
        }

        [HttpGet]
        [Authorize]
        [Route("resume/parse/questions/{resumeParseGuid:guid}")]
        public async Task<IActionResult> GetResumeQuestions(Guid resumeParseGuid)
        {
            var resumeParse = await _resumeService.GetResumeQuestions(GetSubscriberGuid(), resumeParseGuid);
            return Ok(resumeParse);
        }

        [HttpPost]
        [Authorize]
        [Route("resume/parse/questions/{resumeParseGuid:guid}")]
        public async Task<IActionResult> ResolveProfileMerge([FromBody] List<string> mergeInfo, Guid resumeParseGuid)
        {
            await _resumeService.ResolveProfileMerge(mergeInfo, GetSubscriberGuid(), resumeParseGuid);
            return StatusCode(201);
        }

        #endregion

        #region Skills

        [HttpGet]
        [Route("skills")]
        [Authorize]
        public async Task<IActionResult> GetSubscriberSkills()
        {
            var skills = await _skillservice.GetSkillsBySubscriberGuid(GetSubscriberGuid());
            return Ok(skills);
        }

        [HttpPut]
        [Route("skills")]
        [Authorize]
        public async Task<IActionResult> UpdateSubscriberSkills([FromBody] List<string> skills)
        {
            await _skillservice.UpdateSubscriberSkills(GetSubscriberGuid(), skills);
            return StatusCode(200);
        }

        #endregion

        #region courses 



        //todo 
        [HttpGet]
        [Route("{subscriberGuid}/course")]
        [Authorize]
        public async Task<IActionResult> GetSubscriberCourses(Guid SubscriberGuid, int excludeCompleted, int excludeActive)
        {
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsRecruiterPolicy");

            var rVal = await _subscriberCourseService.GetSubscriberCourses(GetSubscriberGuid(), SubscriberGuid, excludeActive, excludeCompleted, isAuth.Succeeded);            
            return Ok(rVal);
        }


        #endregion

        #region Avatar

        [HttpPut]
        [Route("avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar([FromBody] UpDiddyLib.Domain.Models.FileDto fileDto)
        {
            await _avatarService.UploadAvatar(GetSubscriberGuid(), fileDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("avatar")]
        [Authorize]
        public async Task<IActionResult> DeleteAvatar()
        {
            await _avatarService.RemoveAvatar(GetSubscriberGuid());
            return StatusCode(204);
        }

        [HttpGet]
        [Route("avatar")]
        [Authorize]
        public async Task<IActionResult> GetAvatar()
        {
            var avatarUrl = await _avatarService.GetAvatar(GetSubscriberGuid());
            return Ok(avatarUrl);
        }

        #endregion

        #region CareerPath

        [HttpGet]
        [Route("careerpath/")]
        [Authorize]
        public async Task<IActionResult> GetCareerPath()
        {

            var careerPath = await _profileService.GetSubscriberCareerPath(GetSubscriberGuid());
            if (careerPath != null)
            {
                return Ok(careerPath);
            }
            else 
            {
                return StatusCode(404);
            }
        }

        [HttpPut]
        [Route("careerpath/{careerPath:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateCareerPath(Guid careerPath)
        {

            await _profileService.UpdateSubscriberCareerPath(careerPath, GetSubscriberGuid());
            return StatusCode(200);
        }

        #endregion

    }
}