using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberSkillRepository : IUpDiddyRepositoryBase<SubscriberSkill>
    {
        Task<SubscriberSkill> GetBySubscriberGuidAndSkillGuid(Guid subscriberGuid, Guid skillGuid);
        Task<List<SubscriberSkill>> GetActiveSkillsBySubscriberGuid(Guid subscriberGuid);
        Task<List<SubscriberSkill>> GetAllSkillsBySubscriberGuid(Guid subscriberGuid);
        Task UpdateCandidateSkills(Guid subscriberGuid, List<string> skillNames);
        Task<List<SkillDto>> GetCandidateSkills(Guid subscriberGuid, int limit, int offset, string sort, string order);
    }
}
