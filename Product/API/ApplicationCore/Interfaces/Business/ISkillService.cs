using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISkillService
    {
        Task<List<SkillDto>> GetSkills(int limit, int offset, string sort, string order);
        Task<SkillDto> GetSkill(Guid skillGuid);
        Task CreateSkill(SkillDto skillDto);
        Task UpdateSkill(Guid skillGuid, SkillDto skillDto);
        Task DeleteSkill(Guid skillGuid);
        Task<List<SkillDto>> GetSkillsBySubscriberGuid(Guid subscriberGuid);
        Task UpdateSubscriberSkills(Guid subscriber, List<string> skills);
    }
}
