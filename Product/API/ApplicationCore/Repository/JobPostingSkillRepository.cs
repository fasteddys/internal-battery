using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class JobPostingSkillRepository : UpDiddyRepositoryBase<JobPostingSkill>, IJobPostingSkillRepository
    {
        private UpDiddyDbContext _dbContext;
        public JobPostingSkillRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public List<JobPostingSkill> GetByJobPostingId(int jobPostingId)
        {
            return _dbContext.JobPostingSkill
              .Include(c => c.Skill)
              .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingId)
              .ToList();
        }
    }
}
