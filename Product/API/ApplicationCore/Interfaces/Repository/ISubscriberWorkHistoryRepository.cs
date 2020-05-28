using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberWorkHistoryRepository : IUpDiddyRepositoryBase<SubscriberWorkHistory>
    {

        Task<SubscriberWorkHistory> GetLastEmploymentDetailBySubscriberGuid(Guid subscriberGuid);

        Task<List<SubscriberWorkHistory>> GetWorkHistoryBySubscriberGuid(Guid subscriberGuid, int limit, int offset, string sort, string order);
    }
}
