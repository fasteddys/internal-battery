using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SkillRepository :UpDiddyRepositoryBase<Skill>, ISkillRepository
    {
        private UpDiddyDbContext _dbContext;
        public SkillRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext=dbContext;
        }
        public async Task<IQueryable<Skill>> GetAllSkillsQueryable()
        {
            return await GetAllAsync();
        }

        public async Task<List<Skill>> GetAllSkillsForJobPostings()
        {
            var skills =await (from skill in _dbContext.Skill
                                join jobPostingSkill in _dbContext.JobPostingSkill on skill.SkillId equals jobPostingSkill.SkillId
                                where skill.IsDeleted.Equals(0) && jobPostingSkill.IsDeleted.Equals(0)
                                select skill).Distinct().ToListAsync();

            return skills;
        }
    }
}