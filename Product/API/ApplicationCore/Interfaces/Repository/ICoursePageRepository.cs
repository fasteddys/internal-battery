using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICoursePageRepository : IUpDiddyRepositoryBase<CoursePage>
    {
        Task<IQueryable<CoursePage>> GetAllCoursePagesForCourseSiteAsync(Guid courseSiteGuid);
        Task<IQueryable<CoursePage>> GetPendingCoursePagesForCourseSiteAsync(Guid courseSiteGuid);
    }
}