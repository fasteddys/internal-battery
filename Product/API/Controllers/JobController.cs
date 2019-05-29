using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using System.Net;
using Microsoft.SqlServer.Server;
// Use alias to avoid collisions on classname such as "Company"
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using AutoMapper.Configuration;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using UpDiddyApi.ApplicationCore;
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyApi.Helpers.Job;
using System.Security.Claims;
using UpDiddyLib.Shared.GoogleJobs;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Controllers
{

    public class JobController : ControllerBase
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

        #region constructor 
        public JobController(IServiceProvider services)

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>(); ;

            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory);
        }

        #endregion


        #region job statistics 


        /// <summary>
        /// Get all job postings for a subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/scrape-statistics/{numRecords}")]
        public IActionResult GetJobsForSubscriber(int numRecords)
        {
            var stats = _repositoryWrapper.JobSiteScrapeStatistic.GetJobScrapeStatisticsAsync(numRecords).Result;
            return Ok(_mapper.Map<IList<JobSiteScrapeStatisticDto>>(stats));
        }


        #endregion

        #region job posting favorites


        [HttpGet]
        [Authorize]
        [Route("api/[controller]/favorite/subscriber/{subscriberGuid}")]
        public IActionResult GetJobFavoritesForSubscriber(Guid subscriberGuid)
        {

            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subscriberGuid == null || subscriberGuid != subsriberGuidClaim)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job Posting Favorites can only be viewed by their owner" });

            List<JobPosting> subscriberJobPostingFavorites = JobPostingFavoriteFactory.GetJobPostingFavoritesForSubscriber(_db, subscriberGuid);
            return Ok(_mapper.Map<List<JobPostingDto>>(subscriberJobPostingFavorites));
        }



        [HttpDelete]
        [Authorize]
        [Route("api/[controller]/favorite/{jobPostingFavoriteGuid}")]
        public IActionResult DeleteJobPostingFavorite(Guid jobPostingFavoriteGuid)
        {
            JobPostingFavorite jobPostingFavorite = null;
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPostingFavorite started at: {DateTime.UtcNow.ToLongDateString()} for posting {jobPostingFavoriteGuid}");

                if (jobPostingFavoriteGuid == null)
                    return BadRequest(new { code = 400, message = "No job posting favorite identifier was provided" });

                jobPostingFavorite = JobPostingFavoriteFactory.GetJobPostingFavoriteByGuidWithRelatedObjects(_db, jobPostingFavoriteGuid);
                if (jobPostingFavorite == null)
                    return NotFound(new { code = 404, message = $"Job posting favorite {jobPostingFavoriteGuid} does not exist" });

                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPostingFavorite.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Can only delete your own job posting favorites" });

                jobPostingFavorite.IsDeleted = 1;
                jobPostingFavorite.ModifyDate = DateTime.UtcNow;

                _db.SaveChanges();

                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPostingFavorite completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPostingFavorite exception : {ex.Message} while deleting posting {jobPostingFavoriteGuid}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"JobPosting {jobPostingFavorite.JobPostingFavoriteGuid}  has been deleted " });
        }



        [HttpPost]
        [Authorize]
        [Route("api/[controller]/favorite")]
        public IActionResult CreateJobPostingFavorite([FromBody] JobPostingFavoriteDto jobPostingFavoriteDto)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:JobPostingFavoriteDto started at: {DateTime.UtcNow.ToLongDateString()}");
                if (jobPostingFavoriteDto == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job posting favorite required" });

                // Validate request 
                Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                JobPosting jobPosting = null;
                Subscriber subscriber = null;
                string ErrorMsg = string.Empty;
                if (JobPostingFavoriteFactory.ValidateJobPostingFavorite(_db, jobPostingFavoriteDto, subscriberGuid, ref subscriber, ref jobPosting, ref ErrorMsg) == false)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ErrorMsg });
                else
                {
                    JobPostingFavorite jobPostingFavorite = _db.JobPostingFavorite.Where(jpf => jpf.SubscriberId == subscriber.SubscriberId && jpf.JobPostingId == jobPosting.JobPostingId).FirstOrDefault();
                    if(jobPostingFavorite == null)
                    {
                        jobPostingFavorite = JobPostingFavoriteFactory.CreateJobPostingFavorite(subscriber, jobPosting);
                        _db.JobPostingFavorite.Add(jobPostingFavorite);
                    }
                    else 
                    {
                        jobPostingFavorite.IsDeleted = 0;
                        jobPostingFavorite.ModifyDate = DateTime.UtcNow;
                    }

                    try
                    {
                        _db.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
                    }

                    _syslog.Log(LogLevel.Information, $"***** JobController:JobPostingFavoriteDto completed at: {DateTime.UtcNow.ToLongDateString()}");
                    return Ok(new JobPostingFavoriteDto() { JobPostingFavoriteGuid = jobPostingFavorite.JobPostingFavoriteGuid });
                }
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:JobPostingFavoriteDto exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }

        #endregion

        #region job crud 

        /// <summary>
        /// Get all job postings for a subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/subscriber/{subscriberGuid}")]
        public IActionResult GetJobsForSubscriber(Guid subscriberGuid)
        {


            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subscriberGuid == null || subscriberGuid != subsriberGuidClaim)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting can only be viewed by their owner" });

            List<JobPosting> jobPostings = JobPostingFactory.GetJobPostingsForSubscriber(_db, subscriberGuid);

            return Ok(_mapper.Map<List<JobPostingDto>>(jobPostings));
        }


        /// <summary>
        /// Delete a job posting 
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/{jobPostingGuid}")]
        public IActionResult DeleteJobPosting(Guid jobPostingGuid)
        {

            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting started at: {DateTime.UtcNow.ToLongDateString()} for posting {jobPostingGuid}");

                if (jobPostingGuid == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "No job posting identifier was provided" });

                string ErrorMsg = string.Empty;
                if (JobPostingFactory.DeleteJob(_db, jobPostingGuid, ref ErrorMsg, _syslog, _mapper, _configuration) == false)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ErrorMsg });
                else
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"JobPosting {jobPostingGuid}  has been deleted " });
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting exception : {ex.Message} while deleting posting {jobPostingGuid}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }

        /// <summary>
        /// Update a job posting 
        /// </summary>
        /// <param name="jobPostingDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]")]
        public IActionResult UpdateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {
            try
            {
                // validate request 
                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPostingDto.Recruiter.Subscriber == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");
                // update the job posting 
                string ErrorMsg = string.Empty;
                bool UpdateOk = JobPostingFactory.UpdateJobPosting(_db, jobPostingDto.JobPostingGuid.Value, jobPostingDto, ref ErrorMsg);
                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
                if (UpdateOk)
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPostingDto.JobPostingGuid.Value}" });
                else
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ErrorMsg });
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }


        /// <summary>
        /// Create a job posting 
        /// </summary>
        /// <param name="jobPostingDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]")]
        public IActionResult CreateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {
            try
            {
                // Validate request 
                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPostingDto.Recruiter.Subscriber == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                if (jobPostingDto == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting is required" });

                Recruiter recruiter = RecruiterFactory.GetRecruiterBySubscriberGuid(_db, jobPostingDto.Recruiter.Subscriber.SubscriberGuid.Value);

                if (recruiter == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = $"Recruiter {jobPostingDto.Recruiter.Subscriber.SubscriberId} rec not found" });


                string errorMsg = string.Empty;
                Guid newPostingGuid = Guid.Empty;
                if (JobPostingFactory.PostJob(_db, recruiter.RecruiterId, jobPostingDto, ref newPostingGuid, ref errorMsg, _syslog, _mapper, _configuration) == true)
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{newPostingGuid}" });
                else
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = errorMsg });

            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }

        /// <summary>
        /// Copy a job posting 
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/{jobPostingGuid}")]
        public IActionResult CopyJob(Guid jobPostingGuid)
        {
            // Get the posting to be copied as an untracked entity
            JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(_db, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "JobPosting not found" });

            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim)
                return BadRequest(new BasicResponseDto() { StatusCode = 401, Description = "Unauthorized to copy posting" });

            jobPosting = JobPostingFactory.CopyJobPosting(_db, jobPosting, _postingTTL);

            return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPosting.JobPostingGuid}" });
        }

        #endregion

        #region job search 

        /// <summary>
        /// Get a specific job posting 
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/[controller]/{jobPostingGuid}")]
        public IActionResult GetJob(Guid jobPostingGuid)
        {
            JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(_db, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "JobPosting not found" });
            JobPostingDto rVal = _mapper.Map<JobPostingDto>(jobPosting);
            /* i would prefer to get the semantic url in automapper, but i ran into a blocker while trying to call the static util method
             * in "MapFrom" while guarding against null refs: an expression tree lambda may not contain a null propagating operator
             * .ForMember(jp => jp.SemanticUrl, opt => opt.MapFrom(src => Utils.GetSemanticJobUrlPath(src.Industry?.Name,"","","","","")))
             */
            rVal.SemanticJobPath = Utils.CreateSemanticJobPath(
                jobPosting.Industry?.Name,
                jobPosting.JobCategory?.Name,
                jobPosting.Country,
                jobPosting.Province,
                jobPosting.City,
                jobPostingGuid.ToString());
            return Ok(rVal);
        }

        [HttpGet]
        [Route("api/sitemap/[controller]/")]
        public IActionResult GetAllJobsForSitemap()
        {
            var allJobsForSitemap = JobPostingFactory.GetAllJobPostingsForSitemap(_db);
            return Ok(_mapper.Map<List<JobPostingDto>>(allJobsForSitemap));            
        }

        [HttpGet]
        [Route("api/[controller]/browse-jobs-location/{Country?}/{Province?}/{City?}/{Industry?}/{JobCategory?}/{Skill?}/{PageNum?}")]
        public async Task<IActionResult> JobSearchByLocation(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum)


        {

            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobQueryDto jobQuery = JobQueryHelper.CreateJobQuery(Country, Province, City, Industry, JobCategory, Skill, PageNum, PageSize, Request.Query);
            JobSearchResultDto rVal = _cloudTalent.Search(jobQuery);

            // don't let this stop job search from returning
            try
            {
                ClientEvent ce = await _cloudTalent.CreateClientEventAsync(rVal.RequestId, ClientEventType.Impression, rVal.Jobs.Select(job => job.CloudTalentUri).ToList<string>());
                rVal.ClientEventId = ce.EventId;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "JobController.JobSearchByLocation:: Unable to record client event");
            }

            return Ok(rVal);
        }

        [HttpGet]
        [Route("api/[controller]/browse-jobs-industry/{Industry?}/{JobCategory?}/{Country?}/{Province?}/{City?}/{Skill?}/{PageNum?}")]
        public IActionResult JobSearchIndustry(string Industry, string JobCategory, string Country, string Province, string City, string Skill, int PageNum)
        {

            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobQueryDto jobQuery = JobQueryHelper.CreateJobQuery(Country, Province, City, Industry, JobCategory, Skill, PageNum, PageSize, Request.Query);
            JobSearchResultDto rVal = _cloudTalent.Search(jobQuery);
            return Ok(rVal);
        }


        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult JobSearch([FromBody] JobQueryDto jobQueryDto)
        {
            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobSearchResultDto rVal = _cloudTalent.Search(jobQueryDto);
            return Ok(rVal);
        }

        /// <summary>
        /// Get a specific job posting for an expired job to use its context for comparables.
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/[controller]/expired/{jobPostingGuid}")]
        public IActionResult GetExpiredJob(Guid jobPostingGuid)
        {
            JobPosting jobPosting = JobPostingFactory.GetExpiredJobPostingByGuid(_db, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "JobPosting not found" });

            return Ok(_mapper.Map<JobPostingDto>(jobPosting));
        }

        #endregion

        #region Google Cloud Talent Client Event Tracking/Report
        [HttpPost]
        [Route("api/[controller]/{jobGuid}/track")]
        public async Task<IActionResult> JobEvent(Guid jobGuid, [FromBody] GoogleCloudEventsTrackingDto dto)
        {
            JobPosting jp = await _db.JobPosting.Where(x => x.JobPostingGuid == jobGuid).FirstOrDefaultAsync();
            ClientEvent ce = await _cloudTalent.CreateClientEventAsync(dto.RequestId, dto.Type, new List<string>() { jp.CloudTalentUri }, dto.ParentClientEventId);
            return Ok(new GoogleCloudEventsTrackingDto {
                RequestId = ce.RequestId,
                ClientEventId = ce.EventId
            });
        }
        #endregion

        #region Misc Job Utilities

        [HttpGet("api/[controller]/categories")]
        public async Task<IList<JobCategory>> GetJobCategories()
        {
            return _repositoryWrapper.JobCategoryRepository.GetAllAsync().Result.ToList();
        }

        #endregion
    }
}