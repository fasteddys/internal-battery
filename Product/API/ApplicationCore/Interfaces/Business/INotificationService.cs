using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface INotificationService
    {
        Task<Guid> CreateNotification(Guid subscriberGuid, NotificationCreateDto notificationCreateDto);

        Task DeleteNotification(Guid subscriberGuid, Guid notificationGuid);

        Task UpdateNotification(Guid subscriberGuid, NotificationDto notification, Guid notificationGuid);

        Task<List<NotificationDto>> GetNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");

        Task<NotificationDto> GetNotification(Guid notificationGuid);

        Task<bool> SendNotifcation(Guid subscriberGuid, Guid notificationGuid);

    }        
}
