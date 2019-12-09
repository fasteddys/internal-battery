using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore;
using UpDiddyLib.Helpers;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;

namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
    public class CoursesController : BaseApiController
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
        private readonly ICourseService _courseService;
        private readonly ICourseFavoriteService _courseFavoriteService;


        public CoursesController(UpDiddyDbContext db
        , IMapper mapper
        , IConfiguration configuration
        , ISysEmail sysemail
        , IHttpClientFactory httpClientFactory
        , ILogger<CourseController> syslog
        , IDistributedCache distributedCache
        , IHangfireService hangfireService
        , IRepositoryWrapper repositoryWrapper
        , ICourseService courseService
        , ICourseFavoriteService courseFavoriteService)
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
            _repositoryWrapper = repositoryWrapper;
            _courseService = courseService;
            _courseFavoriteService = courseFavoriteService;
        }

        [HttpGet]
        [Route("random")]
        public async Task<IActionResult> Random(Guid JobGuid)
        {
            return Ok(await _courseService.GetCoursesRandom(Request.Query));
        }

        [HttpGet]
        [Route("job/{JobGuid:guid}/related")]
        public async Task<IActionResult> Search(Guid JobGuid)
        {
            return Ok(await _courseService.GetCoursesForJob(JobGuid, Request.Query));
        }

        [HttpPost]
        [Route("skill/related")]
        public async Task<IActionResult> SearchBySkills([FromBody] Dictionary<string, int> SkillHistogram)
        {
            return Ok(await _courseService.GetCoursesBySkillHistogram(SkillHistogram, Request.Query));
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses(int limit, int offset, string sort, string order)
        {
            var courses = await _courseService.GetCourses(limit, offset, sort, order);
            return Ok(courses);
        }

        [HttpGet]
        [Route("{course:guid}")]
        public async Task<IActionResult> GetCourse(Guid course)
        {
            var courses = await _courseService.GetCourse(course);
            return Ok(courses);
        }

        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCoursesCount()
        {
            var count = await _courseService.GetCoursesCount();
            return Ok(count);
        }

        [HttpGet]
        [Route("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavoriteCourses(int limit, int offset, string sort, string order)
        {
            var isFavorite = await _courseFavoriteService.GetFavoriteCourses(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(isFavorite);
        }

        [HttpGet]
        [Route("{course}/favorites")]
        [Authorize]
        public async Task<IActionResult> IsCourseAddedToFavorite(Guid course)
        {
            var isFavorite = await _courseFavoriteService.IsCourseAddedToFavorite(GetSubscriberGuid(), course);
            return Ok(isFavorite);
        }

        [HttpPost]
        [Route("{course:guid}/favorites")]
        [Authorize]

        public async Task<IActionResult> AddCourseFavorite(Guid course)
        {
            await _courseFavoriteService.AddToFavorite(GetSubscriberGuid(), course);
            return StatusCode(201);
        }

        [HttpDelete]
        [Route("{course:guid}/favorites")]
        [Authorize]

        public async Task<IActionResult> RemoveCourseFavorite(Guid course)
        {
            await _courseFavoriteService.RemoveFromFavorite(GetSubscriberGuid(), course);
            return StatusCode(204);
        }

        #region Related Entities

        [HttpGet]
        [Route("courses/{course:guid}/related")]
        public async Task<IActionResult> GetRelatedCoursesByCourse(Guid course, int limit = 100, int offset = 0)
        {
            List<RelatedCourseDto> relatedCourses = null;

            relatedCourses = await _courseService.GetCoursesByCourse(course, limit, offset);

            return Ok(relatedCourses);
        }

        [HttpGet]
        [Route("jobs/{job:guid}/related")]
        public async Task<IActionResult> GetRelatedCoursesByJob(Guid job, int limit = 100, int offset = 0)
        {
            List<RelatedCourseDto> relatedCourses = null;

            relatedCourses = await _courseService.GetCoursesByJob(job, limit, offset);

            return Ok(relatedCourses);
        }

        [HttpGet]
        [Route("subscribers/related")]
        public async Task<IActionResult> GetRelatedCoursesForSubscriber(int limit = 100, int offset = 0)
        {
            var subscriber = GetSubscriberGuid();
            if (subscriber == Guid.Empty)
                throw new NotFoundException("Subscriber not found");

            var relatedCourses = await _courseService.GetCoursesBySubscriber(subscriber, limit, offset);

            return Ok(relatedCourses);
        }

        #endregion
    }
}