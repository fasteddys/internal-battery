using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CourseSiteRepository : UpDiddyRepositoryBase<CourseSite>, ICourseSiteRepository
    {
        public CourseSiteRepository(UpDiddyDbContext dbContext) : base(dbContext) { }

        public async Task<IQueryable<CourseSite>> GetAllCourseSitesAsync()
        {
            var courseSites = GetAll();
            return courseSites
                .Include(cs => cs.CoursePages).ThenInclude(cp => cp.CoursePageStatus)
                .Where(cs => cs.IsDeleted == 0).OrderBy(cs => cs.Name);
        }
    }
}