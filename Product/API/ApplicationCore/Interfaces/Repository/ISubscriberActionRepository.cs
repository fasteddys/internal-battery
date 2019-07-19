using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberActionRepository : IUpDiddyRepositoryBase<SubscriberAction>
    {
        Task CreateSubscriberAction(SubscriberAction subscriberAction);
        Task<List<SubscriberAction>> GetSubscriberActionByEntityAndEntityType(int entityTypeId, int? entityId);
    }
}
