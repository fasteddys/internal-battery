using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using UpDiddyApi.Helpers.Job;
using System.Security.Claims;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Controllers
{

    [ApiController]
    public class JobsController : ControllerBase
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
        private readonly IJobService _jobService;
        private readonly IHangfireService _hangfireService;
        private readonly ISubscriberService _subscriberService;
        private readonly IJobPostingService _jobPostingService;


        #region constructor 
        public JobsController(IServiceProvider services, IHangfireService hangfireService)

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);

            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
        }

        #endregion


        [HttpGet]
        [Route("/V2/[controller]/search")]
        public async Task<IActionResult> Search()
        {
            JobSearchSummaryResultDto rVal = await _jobService.SummaryJobSearch(Request.Query);
            return Ok(rVal);
        }


        [HttpPost]
        [Route("/V2/[controller]/{job}/alert")]
        [Authorize]
        public async Task<IActionResult> SaveJobAlert(Guid job)
        {
            if (job == Guid.Empty)
            {
                return BadRequest();
            }
            try
            {
                Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                JobPostingAlertDto jobPostingAlertDbo = await _jobService.SaveJobAlert(job, subscriberGuid);
                if (jobPostingAlertDbo == null)
                {
                    return BadRequest();
                }
                else
                {
                    return StatusCode(201);
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

    }
}