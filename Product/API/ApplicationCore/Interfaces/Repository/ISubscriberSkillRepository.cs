using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberSkillRepository : IUpDiddyRepositoryBase<SubscriberSkill>
    {
        Task<SubscriberSkill> GetBySubscriberGuidAndSkillGuid(Guid subscriberGuid, Guid skillGuid);
        Task<List<SubscriberSkill>> GetActiveSkillsBySubscriberGuid(Guid subscriberGuid);
        Task<List<SubscriberSkill>> GetAllSkillsBySubscriberGuid(Guid subscriberGuid);
    }
}
