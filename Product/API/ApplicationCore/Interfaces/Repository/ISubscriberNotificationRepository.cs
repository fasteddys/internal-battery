using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberNotificationRepository : IUpDiddyRepositoryBase<SubscriberNotification>
    {
        /// <summary>
        /// Retrieves a subscriber notification by the subscriber and notification identifiers.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="notificationGuid"></param>
        /// <returns></returns>
        Task<SubscriberNotification> GetSubscriberNotificationByIdentifiersAsync(Guid subscriberGuid, Guid notificationGuid);
    }
}

