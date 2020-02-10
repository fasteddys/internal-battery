using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISkillService
    {
        Task<SkillListDto> GetSkills(int limit, int offset, string sort, string order);
        Task<List<SkillDto>> GetSkillsByKeyword(string keyword);
        Task<SkillDto> GetSkill(Guid skillGuid);
        Task<Guid> CreateSkill(SkillDto skillDto);
        Task UpdateSkill(Guid skillGuid, SkillDto skillDto);
        Task DeleteSkill(Guid skillGuid);
        Task<List<SkillDto>> GetSkillsByCourseGuid(Guid courseGuid);
        Task<List<SkillDto>> GetSkillsBySubscriberGuid(Guid subscriberGuid);
        Task UpdateCourseSkills(Guid course, List<Guid> skills);
        Task UpdateSubscriberSkills(Guid subscriber, List<string> skills);
        Task UpdateSubscriberSkillsByGuid(Guid subscriber, List<Guid> skills);
    }
}
