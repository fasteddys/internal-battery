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

namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
    public class JobsController : BaseApiController
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
        private readonly IJobPostingService _jobPostingService;
        private readonly IJobAlertService _jobAlertService;
        private readonly IJobFavoriteService _jobFavoriteService;
        private readonly IJobSearchService _jobSearchService;
        private readonly ITrackingService _trackingService;
        private readonly IJobApplicationService _jobApplicationService;
        private readonly IKeywordService _keywordService;

        #region constructor 
        public JobsController(IServiceProvider services
        , IHangfireService hangfireService
        , IJobAlertService jobAlertService
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
            _subscriberService = _services.GetService<ISubscriberService>();
            _jobApplicationService = _services.GetService<IJobApplicationService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalentService = cloudTalentService;

            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
            _jobAlertService = jobAlertService;
            _jobFavoriteService = jobFavoriteService;
            _jobSearchService = jobSearchService;
            _keywordService = keywordService;
        }

        #endregion

        #region CloudTalentTracking

        [HttpPost]
        [Route("{job:guid}/tracking/{requestId}/{clientEventId}")]
        [Authorize]
        public async Task<IActionResult> TrackClientEventJobViewAction(Guid job, string requestId, string clientEventId)
        {
            await _cloudTalentService.TrackClientEventJobViewAction(job, requestId, clientEventId);
            return StatusCode(202);
        }
        #endregion

        #region Job Applications


        [HttpPost]
        [Route("{JobGuid:guid}/applications")]
        [Authorize]
        public async Task<IActionResult> CreateJobApplication([FromBody] ApplicationDto jobApplicationDto, Guid JobGuid)
        {
            var jobApplicationGuid = await _jobApplicationService.CreateJobApplication(GetSubscriberGuid(), JobGuid, jobApplicationDto);
            return StatusCode(201, jobApplicationGuid);
        }

        [HttpGet]
        [Route("{JobGuid:guid}/applications")]
        [Authorize]
        public async Task<IActionResult> HasJobApplication(Guid JobGuid)
        {
            var HasApplied = await _jobApplicationService.HasJobApplication(GetSubscriberGuid(), JobGuid);
            return Ok(HasApplied);
        }


        #endregion

        #region Job Browse 

        [HttpGet]
        [Route("browse-location")]
        public async Task<IActionResult> BrowseJobsByLocation()
        {
            JobBrowseResultDto rVal = null;
            rVal = await _jobService.BrowseJobsByLocation(Request.Query);
            return Ok(rVal);
        }

        #endregion

        #region Job Search

        [HttpGet]
        [Route("search/{JobGuid:guid}")]
        public async Task<IActionResult> GetJob(Guid JobGuid)
        {

            JobDetailDto rVal = await _jobService.GetJobDetail(JobGuid);
            return Ok(rVal);
        }

        [HttpGet]
        [Route("search/keyword")]
        public async Task<IActionResult> GetKeywordSearchTerms(string value)
        {
            var rVal = await _keywordService.GetKeywordSearchTerms(value);
            return Ok(rVal);
        }


        [HttpGet]
        [Route("search/location")]
        public async Task<IActionResult> GetLocationSearchTerms(string value)
        {
            var rVal = await _keywordService.GetLocationSearchTerms(value);
            return Ok(rVal);
        }

        [HttpGet]
        [Route("search")]
        public async Task<ActionResult> Search()
        {
            JobSearchSummaryResultDto rVal = await _jobService.SummaryJobSearch(Request.Query);
            return Ok(rVal);
        }

        [HttpGet]
        [Route("search/count")]
        public async Task<IActionResult> GetActiveJobCount()
        {
            var count = await _jobSearchService.GetActiveJobCount();
            return Ok(count);
        }

        [HttpGet]
        [Route("search/{job:guid}/similar")]
        public async Task<IActionResult> GetSimilarJobs(Guid job)
        {
            var jobs = await _jobSearchService.GetSimilarJobs(job);
            return Ok(jobs);
        }

        [HttpGet]
        [Route("search/state-map")]
        public async Task<IActionResult> GetStateMapData()
        {
            var stateMapdto = await _jobSearchService.GetStateMapData();
            return Ok(stateMapdto);
        }

        #endregion

        #region Job Alert

        [HttpPost]
        [Route("alert")]
        [Authorize]
        public async Task<IActionResult> CreateJobAlert([FromBody] JobAlertDto jobPostingAlertDto)
        {
            var jobAlertGuid = await _jobAlertService.CreateJobAlert(GetSubscriberGuid(), jobPostingAlertDto);
            return StatusCode(201, jobAlertGuid);
        }

        [HttpGet]
        [Route("alert")]
        [Authorize]
        public async Task<IActionResult> GetJobAlerts()
        {
            var jobAlerts = await _jobAlertService.GetJobAlert(GetSubscriberGuid());
            return Ok(jobAlerts);
        }

        [HttpPut]
        [Route("alert/{jobAlert:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateJobAlert([FromBody] JobAlertDto jobPostingAlertDto, Guid jobAlert)
        {
            await _jobAlertService.UpdateJobAlert(GetSubscriberGuid(), jobAlert, jobPostingAlertDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("alert/{jobAlert:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteJobAlert(Guid jobAlert)
        {
            await _jobAlertService.DeleteJobAlert(GetSubscriberGuid(), jobAlert);
            return StatusCode(204);
        }

        #endregion

        #region Job Favorites

        [HttpPut]
        [Route("{job:guid}/favorites")]
        [Authorize]
        public async Task<IActionResult> CreateJobFavorite(Guid job)
        {

            await _jobFavoriteService.AddJobToFavorite(GetSubscriberGuid(), job);
            return StatusCode(204);
        }

        [HttpGet]
        [Route("favorites")]
        [Authorize]
        public async Task<IActionResult> GetJobFavorites()
        {

            var favorites = await _jobFavoriteService.GetJobFavorites(GetSubscriberGuid());
            return Ok(favorites);
        }

        [HttpDelete]
        [Route("{job:guid}/favorites")]
        [Authorize]
        public async Task<IActionResult> DeleteJobFavorite(Guid job)
        {

            await _jobFavoriteService.DeleteJobFavorite(GetSubscriberGuid(), job);
            return StatusCode(204);
        }

        #endregion

        #region Job crud
        
        [HttpPost]
        [Route("admin")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> CreateJob([FromBody] JobCrudDto jobPostingDto)
        {
            Guid newJobGuid = await _jobPostingService.CreateJobPostingForSubscriber(GetSubscriberGuid(), jobPostingDto);
            Response.StatusCode = 201;
            return StatusCode(201, newJobGuid);
        }
        
        [HttpPut]
        [Route("admin/{jobGuid:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> UpdateJob([FromBody] JobCrudDto jobCrudDto, Guid jobGuid)
        {
            if (jobCrudDto.JobPostingGuid == null || jobCrudDto.JobPostingGuid == Guid.Empty)
                throw new JobPostingUpdate("jobPostingGuid in request boddy cannot be null or empty");

            if (jobCrudDto.JobPostingGuid != jobGuid)
                throw new JobPostingUpdate("job property from url does not match jobPostingGuid specified in request body");

            await _jobPostingService.UpdateJobPostingForSubscriber(GetSubscriberGuid(), jobCrudDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("admin/{jobGuid:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> DeleteJob(Guid jobGuid)
        {
            await _jobPostingService.DeleteJobPosting(GetSubscriberGuid(), jobGuid);
            return StatusCode(204);
        }

        [HttpGet]
        [Route("admin/{jobGuid:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetJobAdmin(Guid jobGuid)
        {
            JobCrudDto jobPostingDto = await _jobPostingService.GetJobPostingCrud(GetSubscriberGuid(), jobGuid);
            return Ok(jobPostingDto);
        }

        [HttpGet]
        [Route("admin")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetJobAdminForSubscriber(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {

            JobCrudListDto postings = await _jobPostingService.GetJobPostingCrudForSubscriber(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(postings);
        }

        [HttpPut]
        [Route("admin/{jobGuid:guid}/skills")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> UpdateJobSkills([FromBody] List<UpDiddyLib.Domain.Models.SkillDto> skills, Guid jobGuid)
        {

            await _jobPostingService.UpdateJobPostingSkills(GetSubscriberGuid(), jobGuid, skills);
            return StatusCode(204);
        }

        [HttpGet]
        [Route("admin/{jobGuid:guid}/skills")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetJobSkills(Guid jobGuid)
        {
            return Ok(await _jobPostingService.GetJobPostingSkills(GetSubscriberGuid(), jobGuid));

        }

        #endregion

        #region Related Entities

        [HttpPost]
        [Route("courses/related")]
        public async Task<IActionResult> GetRelatedJobsByCourses([FromBody] List<Guid> courses, int limit = 100, int offset = 0)
        {
            List<RelatedJobDto> relatedJobs = null;
            var subscriber = GetSubscriberGuid();

            if (subscriber != Guid.Empty)
                relatedJobs = await _jobPostingService.GetJobsByCourses(courses, limit, offset, subscriber);
            else
                relatedJobs = await _jobPostingService.GetJobsByCourses(courses, limit, offset);

            return StatusCode(200, relatedJobs);
        }

        [HttpGet]
        [Route("courses/{course:guid}/related")]
        public async Task<IActionResult> GetRelatedJobsByCourse(Guid course, int limit = 100, int offset = 0)
        {
            List<RelatedJobDto> relatedJobs = null;
            var subscriber = GetSubscriberGuid();

            if (subscriber != Guid.Empty)
                relatedJobs = await _jobPostingService.GetJobsByCourse(course, limit, offset, subscriber);
            else
                relatedJobs = await _jobPostingService.GetJobsByCourse(course, limit, offset);

            return StatusCode(200, relatedJobs);
        }

        [HttpGet]
        [Route("subscribers/related")]
        public async Task<IActionResult> GetRelatedJobsForSubscriber(int limit = 100, int offset = 0)
        {
            var subscriber = GetSubscriberGuid();
            if (subscriber == Guid.Empty)
                throw new NotFoundException("Subscriber not found");

            var relatedJobs = await _jobPostingService.GetJobsBySubscriber(subscriber, limit, offset);

            return StatusCode(200, relatedJobs);
        }

        #endregion

        #region CareerPath Recommendations

        [HttpGet]
        [Route("recommendations")]
        [Authorize]
        public async Task<IActionResult> GetCareerPathRecommendations(int limit = 5, int offset = 0)
        {
            var careerPathJobs = await _jobPostingService.GetCareerPathRecommendations(limit, offset, GetSubscriberGuid());
            return Ok(careerPathJobs);
        }

        #endregion

        #region Refer A Friend

        [HttpPost]
        [Authorize]
        [Route("refer")]
        public async Task<IActionResult> ReferAFriend([FromBody] JobReferralDto jobReferral)
        {
            var jobReferralGuid = await _jobService.ReferJobToFriend(jobReferral, GetSubscriberGuid());
            return StatusCode(201, jobReferralGuid);
        }

        #endregion

        #region Job Data Mining

        [HttpGet]
        [Route("job-site-scrape-statistics")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> GetJobSiteScrapeStatistics(int limit = 10, int offset = 0, string sort = "endDate", string order = "descending")
        {
            JobSiteScrapeStatisticsListDto jobSiteScrapeStatistics = await _jobPostingService.GetJobSiteScrapeStatistics(limit, offset, sort, order);
            return Ok(jobSiteScrapeStatistics);
        }

        #endregion
    }
}