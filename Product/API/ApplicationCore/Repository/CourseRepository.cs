using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CourseRepository : UpDiddyRepositoryBase<Course>, ICourseRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public CourseRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Course>> GetCoursesByCareerPathGuid(Guid careerPathGuid)
        {
            return await (from cp in _dbContext.CareerPath
                          join cpc in _dbContext.CareerPathCourse on cp.CareerPathId equals cpc.CareerPathId
                          join c in _dbContext.Course on cpc.CourseId equals c.CourseId
                          select c).ToListAsync();
        }
    }
}
