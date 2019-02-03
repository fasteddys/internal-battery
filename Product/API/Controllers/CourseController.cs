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
        protected readonly ILogger _syslog = null;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ISysEmail _sysemail;
        private readonly IDistributedCache _distributedCache;

        public CourseController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, ILogger<CourseController> syslog, IDistributedCache distributedCache)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _syslog = syslog;
            _httpClientFactory = httpClientFactory;
            _wozInterface = new WozInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            _sysemail = sysemail;
            _distributedCache = distributedCache;
        }

        [HttpPut]
        [Authorize]
        [Route("api/[controller]/update-student-course-status/{FutureSchedule}")]
        public IActionResult UpdateStudentCourseStatus(bool futureSchedule)
        {
            string subscriberGuid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var NumEnrollments = _db.Enrollment
                 .Include(s => s.Subscriber)
                 .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid.ToString() == subscriberGuid && s.CompletionDate == null && s.DroppedDate == null)
                .Count();
            // Short circuit if the user does not have any enrollments 
            if (NumEnrollments == 0)
                return Ok();

            int AgeThresholdInHours = int.Parse(_configuration["ProgressUpdateAgeThresholdInHours"]);
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.UpdateStudentProgress(subscriberGuid, AgeThresholdInHours));

            // Queue another update in 6 hours 
            if (futureSchedule)
                BackgroundJob.Schedule<ScheduledJobs>(j => j.UpdateStudentProgress(subscriberGuid, AgeThresholdInHours), TimeSpan.FromHours(AgeThresholdInHours));

            return Ok();
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
        [Route("api/[controller]/topic/{TopicSlug}")]
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

        [HttpGet]
        [Route("api/[controller]/slug/{CourseSlug}")]
        public IActionResult GetCourse(string CourseSlug)
        {
            // retrieve the course data that we store in our system, including course variant and type
            Course course = _db.Course
                .Include(c => c.Vendor)
                .Include(c => c.CourseVariants).ThenInclude(cv => cv.CourseVariantType)
                .Include(c => c.CourseSkills).ThenInclude(cs => cs.Skill)
                .Where(t => t.IsDeleted == 0 && t.Slug == CourseSlug)
                .FirstOrDefault();

            // not the greatest implementation performance-wise, but the alternative requires JOIN syntax and this is easier to read
            course.CourseSkills = course.CourseSkills.Where(cs => cs.IsDeleted == 0).ToList();
            course.CourseVariants = course.CourseVariants.Where(cv => cv.IsDeleted == 0).ToList();

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
        [Route("api/[controller]/course-variant/{courseVariantGuid}")]
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
        [Route("api/[controller]/campaign/{campaignGuid}")]
        public IActionResult GetCourseByCampaignGuid(Guid campaignGuid)
        {

            var _a = _db.Campaign
                .Join(_db.CampaignCourseVariant,
                    campaign => campaign.CampaignId,
                    campaignCourseVariant => campaignCourseVariant.CampaignId,
                    (campaign, campaignCourseVariant) => new { campaign, campaignCourseVariant })
                .Join(_db.CourseVariant,
                    ccv => ccv.campaignCourseVariant.CourseVariantId,
                    courseVariant => courseVariant.CourseVariantId,
                    (ccv, courseVariant) => new { ccv, courseVariant })
                .Select(m => new {courseId = m.courseVariant.CourseId });

            int courseId = _a.Select(n => n.courseId).FirstOrDefault();

            CourseDto rval = null;
            rval = _db.Course
                .Where(t => t.IsDeleted == 0 && t.CourseId == courseId)
                .ProjectTo<CourseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault();

            return Ok(rval);

            
        }
    }
}