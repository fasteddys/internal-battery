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
        // TODO Jab [Authorize(Policy = "IsRecruiterOrAdmin")]         
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

                // TODO JAB ensure owner is trying to delete posting              
                // Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
 
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
        // TODO Jab [Authorize(Policy = "IsRecruiterOrAdmin")]         
        [Route("api/[controller]")]
        public IActionResult UpdateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {
            try
            {
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
        // TODO Jab [Authorize(Policy = "IsRecruiterOrAdmin")]         
        [Route("api/[controller]")]
        public IActionResult CreateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {                 
            try
            {
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
        [Route("api/[controller]")]
        public IActionResult JobSearch()
        {
            // TODO JAB set properties and use jobQuery
            JobQueryDto jobQuery = new JobQueryDto();
           // jobQuery.Skill = "javascript";
           // jobQuery.DatePublished = "past_24_hours";
            JobSearchResultDto rVal = _cloudTalent.Search(jobQuery);

            return Ok(rVal);
        }

        #endregion
    }
}