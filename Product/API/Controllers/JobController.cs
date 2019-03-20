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


        public JobController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog, IHttpClientFactory httpClientFactory)

        {   
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;
            _postingTTL = int.Parse(configuration["JobPosting:PostingTTLInDays"]);
         
        }

        [HttpPost]
        // TODO Jab [Authorize(Policy = "IsRecruiterOrAdmin")]         
        [Route("api/[controller]")]
        public IActionResult CreateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {
                       
            JobPosting jobPosting = _mapper.Map<JobPosting>(jobPostingDto);
            // use factory method to make sure all the base data values are set just 
            // in case the caller didn't set them
            BaseModelFactory.SetDefaultsForAddNew(jobPosting);
            // map company id 
            if ( jobPostingDto.Company != null )
            {
                Company company = CompanyFactory.GetCompanyByGuid(_db, jobPostingDto.Company.CompanyGuid);
                if (company != null)
                    jobPosting.CompanyId = company.CompanyId;
            }
            // map industry id
            if ( jobPostingDto.Industry != null )
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

            jobPosting.CloudTalentIndexStatus = (int) JobPostingIndexStatus.NotIndexed;
            jobPosting.JobPostingGuid = Guid.NewGuid();
            if (jobPosting.PostingDateUTC < DateTime.UtcNow  ) 
                jobPosting.PostingDateUTC = DateTime.UtcNow;
            if (jobPosting.PostingExpirationDateUTC < DateTime.UtcNow)
            {
                jobPosting.PostingExpirationDateUTC = DateTime.UtcNow.AddDays( _postingTTL );
            }
 

            _db.JobPosting.Add(jobPosting);
            _db.SaveChanges();

            //index job into google 
            CloudTalent ct = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(jobPosting));

            return Ok(_mapper.Map<JobPostingDto>(jobPosting));
        }
 
    }
}