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

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling
{
    public class CourseCrawlingService : ICourseCrawlingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public CourseCrawlingService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<List<CourseSiteDto>> GetCourseSites()
        {
            var courseSites = await _repositoryWrapper.CourseSite.GetAllCourseSitesAsync();

            throw new NotImplementedException();
        }

        public Task<List<CourseSitePublishResultsDto>> PublishCourseData(CourseSiteDto courseSite)
        {
            throw new NotImplementedException();
        }

        public Task<List<CourseSiteScrapeResultsDto>> ScrapeCourseData(CourseSiteDto courseSite)
        {
            // what should this return type be??
            var result = _repositoryWrapper.CourseSite.GetByConditionAsync(cs => cs.CourseSiteGuid == courseSite.CourseSiteGuid);



            throw new NotImplementedException();
        }

        public async Task<List<CourseSiteDto>> GetCourseSitesAsync()
        {
            var courseSites = await _repositoryWrapper.CourseSite.GetAllCourseSitesAsync();
            return _mapper.Map<List<CourseSiteDto>>(courseSites);
        }

    }
}
