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
using UpDiddyApi.Business;
using UpDiddyLib.Helpers;
using System.Net.Http;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class CourseController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private WozInterface _wozInterface = null;
        protected internal ISysLog _syslog = null;
        private IHttpClientFactory _httpClientFactory = null;

        //private readonly CCQueue _queue = null;
        public CourseController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysLog sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;
            //_queue = new CCQueue("ccmessagequeue", _queueConnection);
            _wozInterface = new WozInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
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
            // retrieve the course data that we store in our system, including course variant and type
            Course course = _db.Course
                .Include(c => c.Vendor)
                .Include(c => c.CourseVariants)
                .ThenInclude(cv => cv.CourseVariantType)
                .Where(t => t.IsDeleted == 0 && t.Slug == CourseSlug)
                .FirstOrDefault();

            if (course == null)
                return NotFound();

            CourseDto courseDto = _mapper.Map<CourseDto>(course);

            // if this is a woz course, get the terms of service and course schedule. 
            // todo: replace this logic with factory pattern when we add more vendors?
            if (course.Vendor.Name == "WozU")
            {
                // get the terms of service from WozU
                var tos = _wozInterface.GetTermsOfService();
                courseDto.TermsOfServiceDocumentId = tos.DocumentId;
                courseDto.TermsOfServiceContent = tos.TermsOfService;

                // get start dates from WozU
                var startDateUTCs = _wozInterface.CheckCourseSchedule(course.Code);

                // add them to instrtuctor-led course variants
                courseDto.CourseVariants
                    .Where(cv => cv.CourseVariantType.Name == "Instructor-Led")
                    .ToList()
                    .ForEach(cv =>
                    {
                        cv.StartDateUTCs = startDateUTCs;
                    });
            }
            return Ok(courseDto);
        }

        [HttpGet]
        [Route("api/[controller]/GetCourseByGuid/{courseGuid}")]
        public IActionResult GetCourseByGuid(Guid courseGuid)
        {
            Course course = _db.Course
                .Where(t => t.IsDeleted == 0 && t.CourseGuid == courseGuid)
                .FirstOrDefault();

            if (course == null)
                return NotFound();

            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpGet]
        [Route("api/[controller]/GetCourseVariant/{courseVariantGuid}")]
        public IActionResult GetCourseVariant(Guid courseVariantGuid)
        {
            CourseVariant courseVariant = _db.CourseVariant
                .Include(cv => cv.CourseVariantType)
                .Where(cv => cv.CourseVariantGuid == courseVariantGuid)
                .FirstOrDefault();

            if (courseVariant == null)
                return NotFound();
            else
                return Ok(_mapper.Map<CourseVariantDto>(courseVariant));
        }

        [HttpGet]
        [Route("api/[controller]/CourseId/{CourseId}")]
        public IActionResult GetCourseById(int CourseId)
        {

            Course course = _db.Course
                .Where(t => t.IsDeleted == 0 && t.CourseId == CourseId)
                .FirstOrDefault();

            if (course == null)
                return NotFound();
            return Ok(_mapper.Map<CourseDto>(course));
        }

    }

}