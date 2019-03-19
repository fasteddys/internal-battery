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
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;

        public JobController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ILogger<TopicController> sysLog, IDistributedCache distributedCache)

        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
        }





        [HttpPost]
        // TODO Jab [Authorize(Policy = "IsRecruiterOrAdmin")]         
        [Route("api/[controller]/{jobPostingDto}")]
        public IActionResult CreateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {
                       
            JobPosting jobPosting = _mapper.Map<JobPosting>(jobPostingDto);
            // use factory method to make sure all the base data values are set just 
            // in case the caller didn't set them
            BaseModelFactory.SetDefaultsForAddNew(jobPosting);
            jobPosting.GoogleCloudIndexStatus = (int) JobPostingIndexStatus.NotIndexed;
            _db.JobPosting.Add(jobPosting);

            return Ok(_mapper.Map<SubscriberDto>(jobPosting));
        }

        //TODO JAB Remove test endpoint
        [HttpGet]         
        [Route("api/[controller]")]
        public IActionResult GetJobPosting()
        {

            JobPostingDto posting = new JobPostingDto()
            {
                Title = "C# developers needed",
                Description = "need some devs for something or another",
                PostingDateUTC = DateTime.UtcNow,
                JobPostingGuid = Guid.NewGuid()


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