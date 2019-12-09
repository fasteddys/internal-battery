using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CourseLevelRepository : UpDiddyRepositoryBase<CourseLevel>, ICourseLevelRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public CourseLevelRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CourseLevel>> GetAllCourseLevels()
        {
            return await (from e in _dbContext.CourseLevel
                          where e.IsDeleted == 0
                          select e).ToListAsync();
        }
    }
}
