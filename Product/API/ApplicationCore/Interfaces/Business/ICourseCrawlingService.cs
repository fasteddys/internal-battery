using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICourseCrawlingService
    {
        Task<List<CourseSiteDto>> GetCourseSitesAsync();
        Task<CourseSiteDto> CrawlCourseSiteAsync(string courseSiteName);
        Task<CourseSiteDto> SyncCourseSiteAsync(string courseSiteName);
    }
}
