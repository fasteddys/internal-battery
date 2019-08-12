using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.Models.Views;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface INotificationRepository : IUpDiddyRepositoryBase<Notification>
    {
        IQueryable<Notification> GetAllNonDeleted();

        IQueryable<v_UnreadNotifications> GetUnreadSubscriberNotificationsForEmail(int reminderLookbackInDays);

        IQueryable<v_NotificationReadCounts> GetNotificationReadCounts();
    }
}

