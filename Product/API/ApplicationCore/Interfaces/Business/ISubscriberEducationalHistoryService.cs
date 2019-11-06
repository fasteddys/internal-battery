using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberEducationalHistoryService
    {
        Task <bool> CreateEducationalHistory(SubscriberEducationHistoryDto EducationHistoryDto, Guid subscriberGuid);
        Task<bool> UpdateEducationalHistory(SubscriberEducationHistoryDto EducationHistoryDto, Guid subscriberGuid, Guid educationalHistoryGuid);
        Task<bool> DeleteEducationalHistory(Guid subscriberGuid, Guid educationalHistoryGuid);
        Task<List<SubscriberEducationHistoryDto>> GetEducationalHistory(Guid subscriberGuid);
    }
}
