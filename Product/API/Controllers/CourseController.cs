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

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class CourseController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        //private readonly CCQueue _queue = null;
        public CourseController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            //_queue = new CCQueue("ccmessagequeue", _queueConnection);
        }

        // GET: api/courses
        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {
            IList<CourseDto> rval = null;
            rval = _db.Course
                .Where(t => t.IsDeleted == 0)
                .ProjectTo<CourseDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rval);

        }

        [HttpGet]
        [Route("api/[controller]/{TopicSlug}")]
        public IActionResult Get(string TopicSlug)
        {
            IList<TopicDto> matchingTopic = _db.Topic
                .Where(t => t.Slug == TopicSlug)
                .ProjectTo<TopicDto>(_mapper.ConfigurationProvider)
                .ToList();

            int topicId = 0;
            foreach (TopicDto topic in matchingTopic)
            {
                topicId = topic.TopicId;
            }

            IList<CourseDto> rval = null;
            rval = _db.Course
                .Where(t => t.IsDeleted == 0 && t.TopicId == topicId)
                .ProjectTo<CourseDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rval);

        }
        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/PurchaseCourse/{VendorCode}/{CourseCode}")]
        public IActionResult PurchaseCourse(string VendorCode, string CourseCode)
        {
            // 1) Create Enrollment record
            // 2) Queue Purchase Course Message 
            var Msg = new EnrollmentMessage
            {
                CourseCode = CourseCode,
                VendorGuid = VendorCode,
                Nonce = 1030304343.00
            };
            //_queue.EnQueue<EnrollmentMessage>(Msg);

            return Ok(CourseCode);
        }



        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/EnrollStudent/{EnrollmentGuid}")]
        public IActionResult EnrollStudent(string EnrollmentGuid)
        {

            // 1) Call WOZ API if users does not have an exeter ID
            // 2) Return OK
            return Ok();
        }

        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/CreateSection/{EnrollmentGuid}")]
        public IActionResult CreateSection(string EnrollmentGuid)
        {
            // 1) Call WOZ to see if section exists if not create it 
            // 2) Return OK
            return Ok();
        }

        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/EnrollStudentInSection/{EnrollmentGuid}")]
        public IActionResult EnrollStudentInSection(string EnrollmentGuid)
        {
            // 1) 
            // 2) Return OK
            return Ok();
        }




        [HttpGet]
        [Route("api/[controller]/slug/{CourseSlug}")]
        public IActionResult GetCourse(string CourseSlug)
        {
            
            Course course = _db.Course
                .Where(t => t.IsDeleted == 0 && t.Slug == CourseSlug)
                .FirstOrDefault();

            if (course == null)
                return NotFound();
            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpGet]
        [Route("api/[controller]/guid/{CourseGuid}")]
        public IActionResult GetCourseByGuid(Guid? CourseGuid)
        {

            Course course = _db.Course
                .Where(t => t.IsDeleted == 0 && t.CourseGuid == CourseGuid)
                .FirstOrDefault();

            if (course == null)
                return NotFound();
            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpGet]
        [Route("api/[controller]/guid/{CourseGuid}/variant/{VariantType}")]
        public IActionResult GetCourseVariantPrice(Guid? CourseGuid, string VariantType)
        {
            CourseVariant courseVariant = _db.CourseVariant
                .Where(t => t.IsDeleted == 0 && t.CourseGuid == CourseGuid && t.VariantType == VariantType)
                .FirstOrDefault();
            if (courseVariant == null)
                return Ok("0");
            else
                return Ok(courseVariant.Price.ToString());

        }
    }

}