using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICourseDataMining
    {
        Task<List<CourseSiteScrapeResultsDto>> ScrapeCourseData(CourseSiteDto courseSite);
        Task<List<CourseSitePublishResultsDto>> PublishCourseData(CourseSiteDto courseSite);
    }
}
