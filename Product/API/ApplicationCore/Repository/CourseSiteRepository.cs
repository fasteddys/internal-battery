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

        public async Task<IEnumerable<CourseSite>> GetAllCourseSitesAsync()
        {
            var courseSites = GetAllAsync();
            return await courseSites.Result
                .Where(cs => cs.IsDeleted == 0)
                .ToListAsync();
        }
    }
}
