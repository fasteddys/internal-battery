using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository 
{
    public interface ISkillRepository : IUpDiddyRepositoryBase<Skill>
    {
        IQueryable<Skill> GetAllSkillsQueryable();
        Task<List<Skill>> GetAllSkillsForJobPostings();
    }
}