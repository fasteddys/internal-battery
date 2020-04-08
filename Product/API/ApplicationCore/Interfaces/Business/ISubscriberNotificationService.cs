using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    /// <summary>
    /// Service that executes actions related to Subscriber Notifications.
    /// </summary>
    public interface ISubscriberNotificationService
    {
        Task<NotificationDto> GetNotification(Guid subscriberGuid, Guid notificationGuid);
        Task<Guid> CreateSubscriberNotification(Guid subscriberGuid, Guid notificationGuid, Guid recipientGuid);
        Task<bool> DeleteSubscriberNotification(Guid subscriberGuid, Guid notificationGuid);
        Task<bool> UpdateSubscriberNotification(Guid subscriberGuid, Guid notificationGuid, NotificationDto updateNotification);
        Task<NotificationListDto> GetNotifications(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
    }
}
