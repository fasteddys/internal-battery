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
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyApi.Business;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.Internal;
using System.IO;
using Microsoft.Extensions.Caching.Distributed;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ISysLog _syslog;

        public TopicController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysEmail sysemail, IServiceProvider serviceProvider, IDistributedCache distributedCache)

        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = new SysLog(configuration, sysemail, serviceProvider);
        }
        
        [HttpGet]
        [Route("api/[controller]/VaultUri")]
        public IActionResult VaultUri()
        {
            return Ok(_configuration["Vault"]);
        }

        // GET: api/topics
        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {
            IList<TopicDto> rval = null;
            rval = _db.Topic
                .Where(t => t.IsDeleted == 0)
                .ProjectTo<TopicDto>(_mapper.ConfigurationProvider)
                .ToList();

            // TODO remove test code 
            BackgroundJob.Enqueue<ScheduledJobs>(x => x.UpdateWozStudentLastLogin("71A7156E-173F-4054-83ED-AD6127BAFE87"));
            return Ok(rval);
        }

        // GET: api/topics/id
        [HttpGet]
        [Route("api/[controller]/{TopicId}")]
        public IActionResult Get(int TopicId)
        {
            Topic topic = _db.Topic
                .Where(t => t.IsDeleted == 0 && t.TopicId == TopicId)
                .FirstOrDefault();

            if (topic == null)
                return NotFound();

            return Ok(_mapper.Map<TopicDto>(topic));
        }

        [HttpGet]
        [Route("api/[controller]/slug/{TopicSlug}")]
        public IActionResult Get(string TopicSlug)
        {
         
            Topic topic = _db.Topic
                .Where(t => t.IsDeleted == 0 && t.Slug == TopicSlug)
                .FirstOrDefault();

            if (topic == null)
                return NotFound();

            return Ok(_mapper.Map<TopicDto>(topic));
        }

    }

}