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
                 // important! Init all reference object ids to -1 since further logic will use < 0 to check for 
                 // their validity
                 JobPostingFactory.SetDefaultsForAddNew(jobPosting);
                 // Asscociate related objects that were passed by guid
                 // todo find a more efficient way to do this
                 JobPostingFactory.MapRelatedObjects(_db, jobPosting, jobPostingDto);
                 
                 string msg = string.Empty;
                 if (JobHelper.ValidateJobPosting(jobPosting, ref msg) == false)
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
                 // todo make saving the job posting and skills more efficient with a SP
                 _db.JobPosting.Add(jobPosting);
                 _db.SaveChanges();
                 JobPostingFactory.SavePostingSkills(_db, jobPosting, jobPostingDto);
                 //index job into google 
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

            JobSearchResultDto rVal = _cloudTalent.Search();

            return Ok(rVal);



        }

        #endregion
    }
}