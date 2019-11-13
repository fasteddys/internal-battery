using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISubscriberWorkHistoryService
    {
        Task<bool> AddWorkHistory(SubscriberWorkHistoryDto WorkHistoryDto, Guid subscriberGuid);
        Task<bool> UpdateEducationalHistory(SubscriberWorkHistoryDto EducationHistoryDto, Guid subscriberGuid, Guid workHistoryGuid);
        Task<bool> DeleteWorklHistory(Guid subscriberGuid, Guid workHistoryGuid);
        Task<List<SubscriberWorkHistoryDto>> GetWorkHistory(Guid subscriberGuid);
    }
}
