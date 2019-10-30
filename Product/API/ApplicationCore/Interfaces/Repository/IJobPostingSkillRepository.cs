using UpDiddyApi.Models;
using System.Collections.Generic;
namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobPostingSkillRepository : IUpDiddyRepositoryBase<JobPostingSkill>
    {
        List<JobPostingSkill> GetByJobPostingId(int jobPostingId);
    }
}
