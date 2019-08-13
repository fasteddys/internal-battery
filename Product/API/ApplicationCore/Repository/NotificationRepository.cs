using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.Views;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public class NotificationRepository : UpDiddyRepositoryBase<Notification>, INotificationRepository
    {
        UpDiddyDbContext _dbContext;
        public NotificationRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Notification> GetAllNonDeleted()
        {
            return _dbContext.Set<Notification>().IgnoreQueryFilters().Where(n => n.IsDeleted == 0).AsNoTracking();
        }

        public IQueryable<v_UnreadNotifications> GetUnreadSubscriberNotificationsForEmail(int reminderLookbackInDays)
        {
            /* I am aware of the desire to keep DbContext out of our repository classes and have them appear only in UpDiddyRepositoryBase. I 
             * attempted to create a generic method in UpDiddyRepositoryBase that would accept a generic type for `DbContext.Query`, however I
             * was unable to get it to work since the type returned is specific to the implementation (e.g. Notification) and different than
             * the entity for which the repository was created (e.g. v_UnreadNotifications). If you believe that there is a better way to do 
             * this, please provide a working example (or better yet, create a PR targeting this branch which addresses the issue). 
             */
            var spParams = new object[] { new SqlParameter("@ReminderLookbackInDays", reminderLookbackInDays) };
            return _dbContext.Query<v_UnreadNotifications>().FromSql(@"EXEC [dbo].[System_Get_UnreadSubscriberNotifications] @ReminderLookbackInDays", spParams);
        }

        public IQueryable<v_NotificationReadCounts> GetNotificationReadCounts()
        {
            return _dbContext.NotificationReadCounts;
        }
    }
}
