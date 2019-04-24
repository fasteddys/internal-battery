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

        #region constructor 
        public JobController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog, IHttpClientFactory httpClientFactory)

        {    
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;
            _postingTTL = int.Parse(configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory);
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

            List<JobPostingFavorite> jobPostingFavorites = JobPostingFavoriteFactory.GetJobPostingFavoritesForSubscriber(_db, subscriberGuid);
            return Ok(_mapper.Map<List<JobPostingFavorite>>(jobPostingFavorites));
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
                if (jobPostingFavoriteDto == null )
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job posting favorite required" });

                // Validate request 
                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                // validate the user is trying to create a favorite for themselves 
                if (jobPostingFavoriteDto.Subscriber == null || jobPostingFavoriteDto.Subscriber.SubscriberGuid == null || jobPostingFavoriteDto.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Cannot create job favorites for other users" });

                JobPosting jobPosting = null;
                Subscriber subscriber = null;
                string ErrorMsg = string.Empty;
                if (JobPostingFavoriteFactory.ValidateJobPostingFavorite(_db, jobPostingFavoriteDto, subsriberGuidClaim, ref subscriber, ref jobPosting, ref ErrorMsg) == false )               
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ErrorMsg });                
                else
                {
                    JobPostingFavorite jobPostingFavorite = JobPostingFavoriteFactory.CreateJobPostingFavorite(subscriber, jobPosting);
                    _db.JobPostingFavorite.Add(jobPostingFavorite);
                    try
                    {
                        _db.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.HResult == -2146232060)
                            return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job is already a favorite" });
                        else
                            return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
                    }

                    _syslog.Log(LogLevel.Information, $"***** JobController:JobPostingFavoriteDto completed at: {DateTime.UtcNow.ToLongDateString()}");
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPostingFavorite.JobPostingFavoriteGuid}" });
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
            if ( subscriberGuid == null ||   subscriberGuid != subsriberGuidClaim)
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
            JobPosting jobPosting = null;
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting started at: {DateTime.UtcNow.ToLongDateString()} for posting {jobPostingGuid}");

                if (jobPostingGuid == null)
                    return BadRequest(new { code = 400, message = "No job posting identifier was provided" });

                jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(_db, jobPostingGuid);
                if (jobPosting == null)
                    return NotFound(new { code = 404, message = $"Job posting {jobPostingGuid} does not exist" });

                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPosting.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                // queue a job to delete the posting from the job index and mark it as deleted in sql server
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteJob(jobPosting.JobPostingGuid));
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting exception : {ex.Message} while deleting posting {jobPostingGuid}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"JobPosting {jobPosting.JobPostingGuid}  has been deleted " });
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
                // Validate request 
                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPostingDto.Subscriber == null || jobPostingDto.Subscriber.SubscriberGuid == null || jobPostingDto.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");
                // Retreive the current state of the job posting 
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(_db, jobPostingDto.JobPostingGuid.Value);
                if (jobPosting == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = $"{jobPostingDto.JobPostingGuid} is not a valid jobposting guid" });

                try
                {
                    JobPostingFactory.UpdateJobPosting(_db, jobPosting, jobPostingDto);
                }
                catch ( Exception ex)
                {
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = $"{jobPostingDto.JobPostingGuid} could not be updated.  Error: {ex.Message}" });
                }
                
                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
                

                return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPosting.JobPostingGuid}" });

 
           
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
                if (jobPostingDto.Subscriber == null || jobPostingDto.Subscriber.SubscriberGuid == null || jobPostingDto.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                if (jobPostingDto == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting is required" });

                _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");
                //todo move code below to factory method 
                JobPosting jobPosting = _mapper.Map<JobPosting>(jobPostingDto);
                // use factory method to make sure all the base data values are set just 
                // in case the caller didn't set them
                BaseModelFactory.SetDefaultsForAddNew(jobPosting);
                // important! Init all reference object ids to null since further logic will use < 0 to check for 
                // their validity
                JobPostingFactory.SetDefaultsForAddNew(jobPosting);
                // Asscociate related objects that were passed by guid
                // todo find a more efficient way to do this
                JobPostingFactory.MapRelatedObjects(_db, jobPosting, jobPostingDto);

                string msg = string.Empty;

                if (JobPostingFactory.ValidateJobPosting(jobPosting, _configuration, ref msg) == false)
                {
                    var response = new BasicResponseDto() { StatusCode = 400, Description = msg };
                    _syslog.Log(LogLevel.Warning, "JobPostingController.CreateJobPosting:: Bad Request {Description} {JobPosting}", response.Description, jobPostingDto);
                    return BadRequest(response);
                }

                jobPosting.CloudTalentIndexStatus = (int)JobPostingIndexStatus.NotIndexed;
                jobPosting.JobPostingGuid = Guid.NewGuid();
                // set expiration date 
                if (jobPosting.PostingDateUTC < DateTime.UtcNow)
                    jobPosting.PostingDateUTC = DateTime.UtcNow;
                if (jobPosting.PostingExpirationDateUTC < DateTime.UtcNow)
                {
                    jobPosting.PostingExpirationDateUTC = DateTime.UtcNow.AddDays(_postingTTL);
                }
                // save the job to sql server 
                // todo make saving the job posting and skills more efficient with a stored procedure 
                _db.JobPosting.Add(jobPosting);
                _db.SaveChanges();
                JobPostingFactory.SavePostingSkills(_db, jobPosting, jobPostingDto);
                //index active jobs into google 
                if (jobPosting.JobStatus == (int)JobPostingStatus.Active)
                    BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(jobPosting.JobPostingGuid));

                _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPosting.JobPostingGuid}" });
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
            if (jobPosting.Subscriber.SubscriberGuid != subsriberGuidClaim)
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
           if ( jobPosting == null )
                return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "JobPosting not found" });

            return Ok(_mapper.Map<JobPostingDto>(jobPosting));
        }

        [HttpGet]
        [Route("api/[controller]/browse-jobs-location/{Country?}/{Province?}/{City?}/{Industry?}/{JobCategory?}/{Skill?}/{PageNum?}")]
        public IActionResult JobSearchByLocation(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum)
        {

            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobQueryDto jobQuery = JobQueryHelper.CreateJobQuery(Country, Province, City, Industry, JobCategory, Skill, PageNum, PageSize,  Request.Query);            
            JobSearchResultDto rVal = _cloudTalent.Search(jobQuery);
            return Ok(rVal);
        }

        [HttpGet]
        [Route("api/[controller]/browse-jobs-industry/{Industry?}/{JobCategory?}/{Country?}/{Province?}/{City?}/{Skill?}/{PageNum?}")]
        public IActionResult JobSearchIndustry(string Industry, string JobCategory, string Country, string Province, string City,  string Skill, int PageNum)
        {

            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobQueryDto jobQuery = JobQueryHelper.CreateJobQuery(Country, Province, City, Industry, JobCategory, Skill, PageNum, PageSize, Request.Query);
            JobSearchResultDto rVal = _cloudTalent.Search(jobQuery);
            return Ok(rVal);
        }
 
        #endregion
    }
}