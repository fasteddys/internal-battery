using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;


namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberEducationHistoryRepository : IUpDiddyRepositoryBase<SubscriberEducationHistory>
    {
        Task<List<SubscriberEducationHistory>> GetEducationalHistoryBySubscriberGuid(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task UpdateCandidateEducationAndTraining(int subscriberId, SubscriberEducationAssessmentsDto subscriberEducationAssessmentsDto);
    }
}
