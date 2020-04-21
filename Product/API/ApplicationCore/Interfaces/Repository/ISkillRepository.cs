using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISkillRepository : IUpDiddyRepositoryBase<Skill>
    {
        IQueryable<Skill> GetAllSkillsQueryable();
        Task<List<Skill>> GetAllSkillsForJobPostings();
        Task<List<Skill>> GetBySubscriberGuid(Guid subscriberGuid);
        Task<Skill> GetByName(string name);
        Task<List<Skill>> GetByTopicGuid(Guid topicGuid);
        Task<List<Skill>> GetByCourseGuid(Guid courseGuid);
        Task<List<SkillDto>> GetProfileSkillsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task DeleteSkillsFromProfileForRecruiter(Guid subscriberGuid, List<Guid> profileSkillGuids);
        Task<List<Guid>> AddSkillsToProfileForRecruiter(Guid subscriberGuid, List<Guid> skillGuids, Guid profileGuid);
        Task UpdateProfileSkillsForRecruiter(Guid subscriberGuid, List<Guid> skillGuids, Guid profileGuid);
        Task<Guid> GetProfileGuidByProfileSkillGuids(List<Guid> profileSkillGuids);
    }
}