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
// Use alias to avoid collistions on classname such as "Company"
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

        #region job crud 
        
        [HttpDelete]
        [Authorize(Policy = "IsRecruiterOrAdmin")]         
        [Route("api/[controller]/{jobPostingGuid}")]
        public IActionResult DeleteJobPosting(Guid jobPostingGuid)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting started at: {DateTime.UtcNow.ToLongDateString()} for posting {jobPostingGuid}");

                if (jobPostingGuid == null)
                    return BadRequest(new { code = 400, message = "No job posting identifier was provided" });

                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(_db, jobPostingGuid);
                if (jobPosting == null)
                    return NotFound(new { code = 404, message = $"Job posting {jobPostingGuid} does not exist" });

                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPosting.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                // queue a job to delete the posting from the job index and mark it as deleted in sql server
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteJob(jobPosting.JobPostingGuid));
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
            catch ( Exception ex )
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting exception : {ex.Message} while deleting posting {jobPostingGuid}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }            
            return Ok();
        }
      
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
                JobPosting tempJobPosting = JobPostingFactory.GetJobPostingByGuid(_db,jobPostingDto.JobPostingGuid.Value);
                if (tempJobPosting == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = $"{jobPostingDto.JobPostingGuid} is not a valid jobposting guid" });
                // map from jobPostingDto to jobPosting
                JobPosting jobPosting = _mapper.Map<JobPosting>(jobPostingDto);
                // Snag the key info for the job posting 
                jobPosting.JobPostingId = tempJobPosting.JobPostingId;
                // Unattach temp job posting from EF
                _db.Entry(tempJobPosting).State = EntityState.Detached;
                // attach the updated version of the  jobposting object to EF 
                _db.JobPosting.Attach(jobPosting);
                // mark the entity as dirty 
                _db.Entry(jobPosting).State = EntityState.Modified;
                // use factory method to make sure all the base data values are set just 
                // in case the caller didn't set them
                BaseModelFactory.SetDefaultsForAddNew(jobPosting);
                // important! Init all reference object ids to null since further logic will use < 0 to check for 
                // their validity
                JobPostingFactory.SetDefaultsForAddNew(jobPosting);
                // copy the keys of the related objects to the updated job posting 
                JobPostingFactory.MapRelatedObjects(jobPosting, tempJobPosting);
                // validate job posting
                string msg = string.Empty;
                if (JobPostingFactory.ValidateUpdatedJobPosting(jobPosting, ref msg) == false)
                {
                    var response = new BasicResponseDto() { StatusCode = 400, Description = msg };
                    _syslog.Log(LogLevel.Warning, "JobPostingController.CreateJobPosting:: Bad Request {Description} {JobPosting}", response.Description, jobPostingDto);
                    return BadRequest(response);
                }
                jobPosting.CloudTalentIndexStatus = (int)JobPostingIndexStatus.UpdateIndexPending;               
                // save the update jobposting to sql server 
                _db.SaveChanges();
                JobPostingFactory.UpdatePostingSkills(_db, jobPosting, jobPostingDto);
                // index active jobs in cloud talent 
                if (jobPosting.JobStatus == (int)JobPostingStatus.Active)
                    BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentUpdateJob(jobPosting.JobPostingGuid));

                return Ok(_mapper.Map<JobPostingDto>(jobPosting));
                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
            catch ( Exception ex )
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }


        [HttpPost]
        [Authorize(Policy = "IsRecruiterOrAdmin")]         
        [Route("api/[controller]")]
        public IActionResult CreateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {                 
            try
            {
                // Validate request 
                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if ( jobPostingDto.Subscriber == null || jobPostingDto.Subscriber.SubscriberGuid == null || jobPostingDto.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                if ( jobPostingDto == null )
                     return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting is required"});

                 _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");
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
                 if (JobPostingFactory.ValidateJobPosting(jobPosting, ref msg) == false)
                 {
                     var response = new BasicResponseDto() { StatusCode = 400, Description = msg };
                     _syslog.Log(LogLevel.Warning, "JobPostingController.CreateJobPosting:: Bad Request {Description} {JobPosting}", response.Description, jobPostingDto);
                     return BadRequest(response);
                 }

                 jobPosting.CloudTalentIndexStatus = (int)JobPostingIndexStatus.NotIndexed;
                 jobPosting.JobPostingGuid = Guid.NewGuid();
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
                 return Ok(_mapper.Map<JobPostingDto>(jobPosting));
            }
            catch(Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }

        #endregion

        #region job search 

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