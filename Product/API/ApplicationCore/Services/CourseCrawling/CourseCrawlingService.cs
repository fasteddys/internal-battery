using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.Models;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common;
using UpDiddyApi.ApplicationCore.Factory;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling
{
    public class CourseCrawlingService : ICourseCrawlingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ISovrenAPI _sovrenApi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CourseCrawlingService> _logger;

        public CourseCrawlingService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration, ISovrenAPI sovrenApi, ILogger<CourseCrawlingService> logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _sovrenApi = sovrenApi;
            _logger = logger;
        }

        public async Task<List<CourseSiteDto>> GetCourseSitesAsync()
        {
            var courseSites = await _repositoryWrapper.CourseSite.GetAllCourseSitesAsync();
            return _mapper.Map<List<CourseSiteDto>>(courseSites);
        }

        public async Task<CourseSiteDto> CrawlCourseSiteAsync(string courseSiteName)
        {
            // load the course site
            var query = await _repositoryWrapper.CourseSite.GetByConditionAsync(x => x.Name == courseSiteName);
            var courseSite = query.FirstOrDefault();

            // todo: if course site name is invalid do what?

            // load the course process for the course site
            ICourseProcess courseProcess = CourseCrawlingFactory.GetCourseProcess(courseSite, _configuration, _logger, _sovrenApi);

            // load all existing course pages - it is important to retrieve all of them regardless of their CoursePageStatus to avoid FK conflicts on insert and update operations
            var coursePages = await _repositoryWrapper.CoursePage.GetAllCoursePagesForCourseSiteAsync(courseSite.CourseSiteGuid);

            // retrieve all current course pages that are visible on the course site
            List<CoursePage> coursePagesToProcess = courseProcess.DiscoverCoursePages(coursePages.ToList());

            // insert or update the course pages
            foreach (var coursePage in coursePagesToProcess)
            {
                if (coursePage.CoursePageId == 0)
                    _repositoryWrapper.CoursePage.Create(coursePage);
                else
                    _repositoryWrapper.CoursePage.Update(coursePage);
            }

            await _repositoryWrapper.CoursePage.SaveAsync();

            // todo: return a valid course site dto

            // todo: how to prevent multiple crawls from occurring (e.g. user clicking "crawl" more than once, or multiple users clicking "crawl" simultaneously)
            throw new NotImplementedException();
        }

        public async Task<CourseSiteDto> SyncCourseSiteAsync(string courseSiteName)
        {
            throw new NotImplementedException();
        }
    }
}
