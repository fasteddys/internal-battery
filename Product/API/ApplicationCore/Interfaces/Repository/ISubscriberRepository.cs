using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberRepository : IUpDiddyRepositoryBase<Subscriber>
    {
        Task<IQueryable<Subscriber>> GetAllSubscribersAsync();

        Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid);
        Task<Subscriber> GetSubscriberByEmailAsync(string email);

        Task<Subscriber> GetSubscriberByIdAsync(int subscriberId);

        Task<List<Subscriber>> GetSubscribersToIndexIntoGoogle(int numSubscribers);

        Task<bool> ForceProfileReindex();

    }
}
