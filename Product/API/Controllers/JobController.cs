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

namespace UpDiddyApi.Controllers
{
 
    public class JobController : ControllerBase
    {
 
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;


        public JobController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog, IHttpClientFactory httpClientFactory)

        {   
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;
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

            jobPosting.GoogleCloudIndexStatus = (int) JobPostingIndexStatus.NotIndexed;
            _db.JobPosting.Add(jobPosting);
            _db.SaveChangesAsync();

            return Ok(_mapper.Map<JobPostingDto>(jobPosting));
        }
        
        //TODO JAB Remove test endpoint

        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult test()
        {
            return Ok();
        }

        
        //TODO JAB Remove test endpoint
        [HttpGet]         
        [Route("api/[controller]/test-job")]
        public IActionResult GetJobPosting()
        {
            CompanyDto company = new CompanyDto()
            {
                CompanyGuid = Guid.NewGuid(),
                CompanyName = "Jim's Test Company",
                CreateDate = DateTime.Now,
                IsDeleted = 0,
                CreateGuid = Guid.NewGuid(),
                ModifyDate = DateTime.Now,
                ModifyGuid = Guid.NewGuid()

            };

            IndustryDto industry = new IndustryDto()
            {
                IndustryGuid = Guid.NewGuid(),
                Name = "Jim's test industry"
            };

            CompensationTypeDto compensationType = new CompensationTypeDto()
            {
                CompensationTypeName = "Hourly",
                IsDeleted = 0,
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                CompensationTypeGuid = Guid.NewGuid(),
                CreateGuid = Guid.NewGuid(),
                ModifyGuid = Guid.NewGuid()


            };

            SecurityClearanceDto securityClearanceDto = new SecurityClearanceDto()
            {
                SecurityClearanceGuid = Guid.NewGuid(),
                Name = "Super Duper Secreter"
            };

            EmploymentTypeDto employmentTypeDto = new EmploymentTypeDto()
            {
                EmploymentTypeGuid = Guid.NewGuid(),
                Name = "Hourly"
            };

        

            JobPostingDto posting = new JobPostingDto()
            {
                Title = "C# developers needed",
                Description = "need some devs for something or another",
                PostingDateUTC = DateTime.UtcNow,
                JobPostingGuid = Guid.NewGuid(),
                IsDeleted = 0,
                Company = company,
                H2Visa = false,
                Industry = industry,
                Compensation = 35.00M,
                CompensationTypeId = 1,
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                PostingExpirationDateUTC = DateTime.Now,
                CreateGuid = Guid.NewGuid(),
                ModifyGuid = Guid.NewGuid(),
                Location = "21204",
                CompensationType = compensationType,
                GoogleCloudIndexStatus = 1,
                GoogleCloudUri = "/projects/projectId/JobGuid",
                TelecommutePercentage = 10,
                ThirdPartyApply = false,
                ThirdPartyApplicationUrl = "http://www.ebay.com",
                SecurityClearance   = securityClearanceDto,
                EmploymentType = employmentTypeDto

            };            
            return Ok(posting);
        }

        


        /*

        CloudTalentSolution.Job j = new Google.Apis.CloudTalentSolution.v3.Data.Job()
        {

           
            
        };

    */


    }
}