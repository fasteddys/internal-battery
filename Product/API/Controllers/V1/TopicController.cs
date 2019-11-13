using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;

        public TopicController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ILogger<TopicController> sysLog, IDistributedCache distributedCache)

        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
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