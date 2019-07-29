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
        Task<IQueryable<Notification>> GetAllNonDeleted();

        Task<IQueryable<v_UnreadNotifications>> GetUnreadSubscriberNotificationsForEmail(int reminderLookbackInDays);

        Task<IQueryable<v_NotificationReadCounts>> GetNotificationReadCounts();
    }
}

