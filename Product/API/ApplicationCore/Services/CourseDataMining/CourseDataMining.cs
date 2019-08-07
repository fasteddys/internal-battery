using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.CourseDataMining
{
    public class CourseDataMining : ICourseDataMining
    {
        public Task<List<CourseSitePublishResultsDto>> PublishCourseData(CourseSiteDto courseSite)
        {
            throw new NotImplementedException();
        }

        public Task<List<CourseSiteScrapeResultsDto>> ScrapeCourseData(CourseSiteDto courseSite)
        {
            throw new NotImplementedException();
        }
    }
}
