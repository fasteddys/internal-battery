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

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ISysLog _syslog;
        public TopicController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysLog sysLog)
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
                .ProjectTo<TopicDto>()
                .ToList();


            // TODO remove test code 
            //  BackgroundJob.Enqueue<WozEnrollmentFlow>(x => x.EnrollStudentWorkItem("00000000-0000-0000-0000-000000000001"));

//            WozInterface wi = new WozInterface(_db,_mapper,_configuration,_syslog);
  //          wi.ReconcileFutureEnrollment("00000000-0000-0000-0000-000000000001");

           
            return Ok(rval) ;
            
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