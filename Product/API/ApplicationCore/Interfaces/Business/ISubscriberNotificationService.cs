using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    /// <summary>
    /// Service that executes actions related to Subscriber Notifications.
    /// </summary>
    public interface ISubscriberNotificationService
    {
        /// <summary>
        /// Performs a logical delete for a subscriber notification.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="notificationGuid"></param>
        /// <returns></returns>
        Task<bool> DeleteSubscriberNotification(Guid subscriberGuid, Guid notificationGuid);
    }
}
