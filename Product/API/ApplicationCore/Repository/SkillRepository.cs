using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SkillRepository : UpDiddyRepositoryBase<Skill>, ISkillRepository
    {
        private UpDiddyDbContext _dbContext;
        public SkillRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Skill> GetAllSkillsQueryable()
        {
            return GetAll();
        }

        public async Task<List<Skill>> GetAllSkillsForJobPostings()
        {
            var skills = await (from skill in _dbContext.Skill
                                join jobPostingSkill in _dbContext.JobPostingSkill on skill.SkillId equals jobPostingSkill.SkillId
                                where skill.IsDeleted.Equals(0) && jobPostingSkill.IsDeleted.Equals(0)
                                select skill).Distinct().ToListAsync();

            return skills;
        }

        public async Task<List<Skill>> GetBySubscriberGuid(Guid subscriberGuid)
        {
            var skills = await (from ss in _dbContext.SubscriberSkill
                                join s in _dbContext.Skill on ss.SkillId equals s.SkillId
                                join su in _dbContext.Subscriber on ss.SubscriberId equals su.SubscriberId
                                where su.SubscriberGuid == subscriberGuid && ss.IsDeleted == 0
                                select s).ToListAsync();

            return skills;
        }

        public async Task<Skill> GetByName(string name)
        {
            return await (from s in _dbContext.Skill
                          where s.SkillName == name
                          select s).FirstOrDefaultAsync();
        }

        public async Task<List<Skill>> GetByTopicGuid(Guid topicGuid)
        {
            return await (from tc in _dbContext.TagCourse
                          join ta in _dbContext.Tag on tc.TagId equals ta.TagId
                          join tt in _dbContext.TagTopic on ta.TagId equals tt.TagId
                          join topic in _dbContext.Topic on tt.TopicId equals topic.TopicId
                          join c in _dbContext.Course on tc.CourseId equals c.CourseId
                          join cs in _dbContext.CourseSkill on c.CourseId equals cs.CourseId
                          join s in _dbContext.Skill on cs.SkillId equals s.SkillId
                          where topic.TopicGuid == topicGuid && topic.IsDeleted == 0 && c.IsDeleted == 0 && cs.IsDeleted == 0
                          select s).Distinct().ToListAsync();
        }


        public async Task<List<Skill>> GetByCourseGuid(Guid courseGuid)
        {
            var skills = await (from ss in _dbContext.CourseSkill
                                join s in _dbContext.Skill on ss.SkillId equals s.SkillId
                                join c in _dbContext.Course on ss.CourseId equals c.CourseId
                                where c.CourseGuid == courseGuid && ss.IsDeleted == 0
                                select s).ToListAsync();

            return skills;
        }
    }
}