using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberRepository : IUpDiddyRepositoryBase<Subscriber>
    {
        IQueryable<Subscriber> GetAllSubscribersAsync();

        Task<SubscriberSourceDto> GetSubscriberSource(int subscriberId);

        Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid);
        Subscriber GetSubscriberByGuid(Guid subscriberGuid);
        Task<Subscriber> GetSubscriberByEmailAsync(string email);
        Subscriber GetSubscriberByEmail(string email);

        Task<Subscriber> GetSubscriberByIdAsync(int subscriberId);

        Task<IList<Partner>>  GetPartnersAssociatedWithSubscriber(int subscriberId);

        Task<int> GetSubscribersCountByStartEndDates(DateTime? startDate = null, DateTime? endDate = null);

        Task UpdateHubSpotDetails(Guid subscriberId, long hubSpotVid);

        Task UpdateHubSpotDetails(int subscriberId, long hubSpotVid);
    }
}
