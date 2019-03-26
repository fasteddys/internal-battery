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
            _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");
            JobPosting jobPosting = _mapper.Map<JobPosting>(jobPostingDto);
            // use factory method to make sure all the base data values are set just 
            // in case the caller didn't set them
            BaseModelFactory.SetDefaultsForAddNew(jobPosting);
            // important! Init all reference object ids to -1 since further logic will use < 0 to check for 
            // their validity
            JobPostingFactory.SetDefaultsForAddNew(jobPosting);
            // map company id 
            if (jobPostingDto.Company != null)
            {
                Company company = CompanyFactory.GetCompanyByGuid(_db, jobPostingDto.Company.CompanyGuid);
                if (company != null)
                    jobPosting.CompanyId = company.CompanyId;
            }
            // map industry id
            if (jobPostingDto.Industry != null)
            {
                Industry industry = IndustryFactory.GetIndustryByGuid(_db, jobPostingDto.Industry.IndustryGuid);
                if (industry != null)
                    jobPosting.IndustryId = industry.IndustryId;
            }
            // map security clearance 
            if (jobPostingDto.SecurityClearance != null)
            {
                SecurityClearance securityClearance = SecurityClearanceFactory.GetSecurityClearanceByGuid(_db, jobPostingDto.SecurityClearance.SecurityClearanceGuid);
                if (securityClearance != null)
                    jobPosting.SecurityClearanceId = securityClearance.SecurityClearanceId;
            }
            // map employment type
            if (jobPostingDto.EmploymentType != null)
            {
                EmploymentType employmentType = EmploymentTypeFactory.GetEmploymentTypeByGuid(_db, jobPostingDto.EmploymentType.EmploymentTypeGuid);
                if (employmentType != null)
                    jobPosting.EmploymentTypeId = employmentType.EmploymentTypeId;
            }            
            // map educational level type
            if (jobPostingDto.EducationLevel != null)
            {
                EducationLevel educationLevel = EducationLevelFactory.GetEducationLevelByGuid(_db, jobPostingDto.EducationLevel.EducationLevelGuid);
                if (educationLevel != null)
                    jobPosting.EducationLevelId = educationLevel.EducationLevelId;
            }        
            // map level experience type
            if (jobPostingDto.ExperienceLevel != null)
            {
                ExperienceLevel experienceLevel = ExperienceLevelFactory.GetExperienceLevelByGuid(_db, jobPostingDto.ExperienceLevel.ExperienceLevelGuid);
                if (experienceLevel != null)
                    jobPosting.ExperienceLevelId = experienceLevel.ExperienceLevelId;
            }



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
            _db.JobPosting.Add(jobPosting);
            _db.SaveChanges();

            //index job into google 

            BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(jobPosting.JobPostingGuid));

            _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
            return Ok(_mapper.Map<JobPostingDto>(jobPosting));
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