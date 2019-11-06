using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore;
using UpDiddyLib.Helpers;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private WozInterface _wozInterface = null;
        protected readonly ILogger _syslog = null;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ISysEmail _sysemail;
        private readonly IDistributedCache _distributedCache;
        private readonly IHangfireService _hangfireService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISkillService _skillservice;


        public SkillsController(IMapper mapper
        , IConfiguration configuration
        , IHangfireService hangfireService
        , ISkillService skillService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _skillservice = skillService;
        }



        [HttpGet]
        [Route("/V2/[controller]/")]
        [Authorize]
        public async Task<IActionResult> GetSkills()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var skills = await _skillservice.GetSkillsBySubscriberGuid(subscriberGuid);
            return Ok(skills);             
        }

        [HttpPost]
        [Route("/V2/[controller]/")]
        [Authorize]
        public async Task<IActionResult> CreateSkill([FromBody] List<SkillDto> skillDto)
        {
             Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
             await _skillservice.CreateSkillForSubscriber(subscriberGuid, skillDto);
        }

        [HttpPost]
        [Route("/V2/[controller]/")]
        [Authorize]
        public async Task<IActionResult> DeleteSkill(Guid skillGuid)
        {
            throw new NotImplementedException();
        }

    }
}