using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CourseFavoriteRepository : UpDiddyRepositoryBase<CourseFavorite>, ICourseFavoriteRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public CourseFavoriteRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<CourseFavorite> GetBySubscriberGuidAndCourseGuid(Guid subscriberGuid, Guid courseGuid)
        {
            return await (from cf in _dbContext.CourseFavorite
                          join s in _dbContext.Subscriber on cf.SubscriberId equals s.SubscriberId
                          join c in _dbContext.Course on cf.CourseId equals c.CourseId
                          where s.SubscriberGuid == subscriberGuid && c.CourseGuid == courseGuid && cf.IsDeleted == 0
                          select cf).FirstOrDefaultAsync();
        }
    }
}
