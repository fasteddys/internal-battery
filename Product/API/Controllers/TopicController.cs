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


namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        public TopicController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
         
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


    }

}