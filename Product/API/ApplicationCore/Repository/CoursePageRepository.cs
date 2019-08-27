using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CoursePageRepository : UpDiddyRepositoryBase<CoursePage>, ICoursePageRepository
    {
        public CoursePageRepository(UpDiddyDbContext dbContext) : base(dbContext) { }

        public async Task<IQueryable<CoursePage>> GetAllCoursePagesForCourseSiteAsync(Guid courseSiteGuid)
        {
            var coursePages = GetAll();
            return coursePages
                .Include(cp => cp.CoursePageStatus)
                .Include(cp => cp.CourseSite)
                .Include(cp => cp.Course)
                .Where(cp => cp.IsDeleted == 0
                    && cp.CourseSite.CourseSiteGuid == courseSiteGuid);
        }

        public async Task<IQueryable<CoursePage>> GetPendingCoursePagesForCourseSiteAsync(Guid courseSiteGuid)
        {
            var coursePages = GetAll();
            return coursePages
                .Include(cp => cp.CoursePageStatus)
                .Include(cp => cp.CourseSite)
                .Include(cp => cp.Course)
                .Where(cp => cp.IsDeleted == 0
                    && (cp.CoursePageStatus.Name == "Create"
                        || cp.CoursePageStatus.Name == "Update"
                        || cp.CoursePageStatus.Name == "Delete")
                    && cp.CourseSite.CourseSiteGuid == courseSiteGuid);
        }
    }
}
