using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface INotificationService
    {
        Task<Guid> CreateNotification(Guid subscriberGuid, NotificationDto notificationDto, Guid? groupGuid);

        Task DeleteNotification(Guid subscriberGuid, Guid notificationGuid);

        Task UpdateNotification(Guid subscriberGuid, NotificationDto notification, Guid notificationGuid);

        Task<NotificationListDto> GetNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");

        Task<NotificationDto> GetNotification(Guid notificationGuid);
    }        
}
