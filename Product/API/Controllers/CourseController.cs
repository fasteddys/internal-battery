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
    public class CourseController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        public CourseController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;

        }

        // GET: api/courses
        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {

            IList<CourseDto> rval = null;
            rval = _db.Course
                .Where(t => t.IsDeleted == 0)
                .ProjectTo<CourseDto>()
                .ToList();

            return Ok(rval);

        }

        [HttpGet]
        [Route("api/[controller]/{TopicSlug}")]
        public IActionResult Get(string TopicSlug)
        {

            IList<TopicDto> matchingTopic = _db.Topic.Where(t => t.Slug == TopicSlug).ProjectTo<TopicDto>().ToList();
            int topicId = 0;
            foreach (TopicDto topic in matchingTopic)
            {
                topicId = topic.TopicId;
            }

            IList<CourseDto> rval = null;
            rval = _db.Course
                .Where(t => t.IsDeleted == 0 && t.TopicId == topicId)
                .ProjectTo<CourseDto>()
                .ToList();

            return Ok(rval);

        }
    }

}