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

        public async Task<IEnumerable<CoursePage>> GetAllCoursePagesForCourseSiteAsync(Guid courseSiteGuid)
        {
            return await GetByConditionAsync(cp => cp.CourseSite.CourseSiteGuid == courseSiteGuid);
        }
    }
}
