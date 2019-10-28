using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyLib.Helpers;
using System.Web;

namespace UpDiddyApi.Controllers
{

    [ApiController]
    public class LookupDataController : ControllerBase
    {


        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected internal ILogger _syslog = null;

        public LookupDataController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
        }
 
        [HttpGet]
        [Route("api/[controller]/Industry")]
        public IActionResult GetIndustries()
        {
            var rVal = _db.Industry
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.Name)
                .ProjectTo<IndustryDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rVal);
        }
 
        [HttpGet]
        [Route("api/[controller]/job-category")]
        public IActionResult GetJobCategories()
        {
            var rVal = _db.JobCategory
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.Name)
                .ProjectTo<JobCategoryDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rVal);
        }
 
        [HttpGet]
        [Route("api/[controller]/education-level")]
        public IActionResult GetEducationLevels()
        {
            var rVal = _db.EducationLevel
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.Level)
                .ProjectTo<EducationLevelDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rVal);
        }



        [HttpGet]
        [Route("api/[controller]/security-clearance")]
        public IActionResult GetSecurityClearances()
        {
            var rVal = _db.SecurityClearance
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.Name)
                .ProjectTo<SecurityClearanceDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rVal);
        }


        [HttpGet]
        [Route("api/[controller]/experience-level")]
        public IActionResult GetExperienceLevels()
        {
            var rVal = _db.ExperienceLevel
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.DisplayName)
                .ProjectTo<ExperienceLevelDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rVal);
        }

        [HttpGet]
        [Route("api/[controller]/employment-type")]
        public IActionResult GetEmploymentTypes()
        {
            var rVal = _db.EmploymentType
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.Name)
                .ProjectTo<EmploymentTypeDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rVal);
        }

        [HttpGet]
        [Route("api/[controller]/compensation-type")]
        public IActionResult GetCompensationTypes()
        {
            var rVal = _db.CompensationType
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.CompensationTypeName)
                .ProjectTo<CompensationTypeDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rVal);
        }


    }
}