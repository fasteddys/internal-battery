using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISkillService
    {
        Task<List<SkillDto>> GetSkillsBySubscriberGuid(Guid subscriberGuid);
        Task CreateSkillForSubscriber(Guid subscriber, List<SkillDto> skills);
    }
}
