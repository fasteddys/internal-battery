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
namespace UpDiddyApi.Controllers
{

    [ApiController]
    public class JobsController : ControllerBase
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

        #region constructor 
        public JobsController(IServiceProvider services
        , IHangfireService hangfireService
        , IJobAlertService jobAlertService
        , IJobFavoriteService jobFavoriteService
        , IJobSearchService jobSearchService
        , ICloudTalentService cloudTalentService
        , ITrackingService trackingService)

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalentService = cloudTalentService;

            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
            _jobAlertService = jobAlertService;
            _jobFavoriteService = jobFavoriteService;
            _jobSearchService = jobSearchService;
        }

        #endregion


        #region CloudTalentTracking

        [HttpPost]
        [Route("/V2/[controller]/{job}/tracking/{requestId}/{clientEventId}")]
        [Authorize]
        public async Task<IActionResult> TrackClientEventJobViewAction(Guid job, string requestId, string clientEventId)
        {
            await _cloudTalentService.TrackClientEventJobViewAction(job, requestId, clientEventId);
            return StatusCode(202);
        }

        #endregion

        #region Job Search

        [HttpGet]
        [Route("/V2/[controller]/search/{JobGuid}")]
        public async Task<IActionResult> GetJob(Guid JobGuid)
        {

                JobDetailDto rVal = await _jobService.GetJobDetail(JobGuid);
                return Ok(rVal);
        }


        [HttpPost]
        [Route("/V2/[controller]/{job}/share")]
        public async Task<IActionResult> Share([FromBody] ShareJobDto shareJobDto, Guid job)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _jobService.ShareJob(subscriberGuid, job, shareJobDto);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("/V2/[controller]/search")]
        public async Task<ActionResult> Search()
        {
            JobSearchSummaryResultDto rVal = await _jobService.SummaryJobSearch(Request.Query);
            return Ok(rVal);
        }

        [HttpGet]
        [Route("/V2/[controller]/search/count")]
        public async Task<IActionResult> GetActiveJobCount()
        {
            var count = await _jobSearchService.GetActiveJobCount();
            return Ok(count);
        }

        [HttpGet]
        [Route("/V2/[controller]/search/{job}/similar")]
        public async Task<IActionResult> GetSimilarJobs(Guid job)
        {
            var jobs = await _jobSearchService.GetSimilarJobs(job);
            return Ok(jobs);
        }

        [HttpGet]
        [Route("/V2/[controller]/search/state-map")]
        public async Task<IActionResult> GetStateMapData()
        {
            var stateMapdto = await _jobSearchService.GetStateMapData();
            return Ok(stateMapdto);
        }


        #endregion

        #region Job Alert

        [HttpPost]
        [Route("/V2/[controller]/alert")]
        [Authorize]
        public async Task<IActionResult> CreateJobAlert([FromBody] JobAlertDto jobPostingAlertDto)
        {

            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _jobAlertService.CreateJobAlert(subscriberGuid, jobPostingAlertDto);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("/V2/[controller]/alert")]
        [Authorize]
        public async Task<IActionResult> GetJobAlerts()
        {

            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var jobAlerts = await _jobAlertService.GetJobAlert(subscriberGuid);
            return Ok(jobAlerts);
        }

        [HttpDelete]
        [Route("/V2/[controller]/alert/{jobAlert}")]
        [Authorize]
        public async Task<IActionResult> DeleteJobAlert(Guid jobAlert)
        {

            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _jobAlertService.DeleteJobAlert(subscriberGuid, jobAlert);
            return StatusCode(204);
        }

        #endregion

        #region Job Favorites

        [HttpPost]
        [Route("/V2/[controller]/{job}/favorites")]
        [Authorize]
        public async Task<IActionResult> CreateJobFavorite(Guid job)
        {

            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _jobFavoriteService.AddJobToFavorite(subscriberGuid, job);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("/V2/[controller]/favorites")]
        [Authorize]
        public async Task<IActionResult> GetJobFavorites()
        {

            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var favorites = await _jobFavoriteService.GetJobFavorites(subscriberGuid);
            return Ok(favorites);
        }

        [HttpDelete]
        [Route("/V2/[controller]/{job}/favorites")]
        [Authorize]
        public async Task<IActionResult> DeleteJobFavorite(Guid job)
        {

            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _jobFavoriteService.DeleteJobFavorite(subscriberGuid, job);
            return StatusCode(204);
        }

        #endregion
    }
}