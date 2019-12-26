using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

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
        Task<bool> DeleteSubscriberNotification(bool isAdmin, Guid subscriberGuid, Guid notificationGuid, Guid recipientGuid);
        Task<NotificationDto> GetNotification(Guid subscriberGuid, Guid notificationGuid);

        Task<Guid> CreateSubscriberNotification(Guid subscriberGuid, Guid notificationGuid, Guid recipientGuid);

        Task<bool> DeleteSubscriberNotification(Guid subscriberGuid, Guid notificationGuid);

        Task<bool> UpdateSubscriberNotification(Guid subscriberGuid, Guid notificationGuid, Guid recipientGuid, NotificationDto updateNotification);

        Task<List<NotificationDto>> GetNotifications(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");

    }
}
