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

        public async Task<List<Course>> GetCoursesByTopicGuid(Guid topicGuid)
        {
            return await (from tc in _dbContext.TagCourse
                          join ta in _dbContext.Tag on tc.TagId equals ta.TagId
                          join tt in _dbContext.TagTopic on ta.TagId equals tt.TagId
                          join topic in _dbContext.Topic on tt.TopicId equals topic.TopicId
                          join c in _dbContext.Course on tc.CourseId equals c.CourseId
                          where c.IsDeleted == 0 && topic.TopicGuid == topicGuid
                          select c).Include(x => x.Vendor).OrderBy(x =>x.SortOrder).ToListAsync();
        }

        public async Task<int> GetCoursesCount()
        {
            return await _dbContext.Course.Where(x => x.IsDeleted == 0).CountAsync();
        }
    }
}
