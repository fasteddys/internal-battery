using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISkillService
    {
        Task<List<SkillDto>> GetSkillsBySubscriberGuid(Guid subscriberGuid);
        Task UpdateSubscriberSkills(Guid subscriber, List<string> skills);
    }
}
