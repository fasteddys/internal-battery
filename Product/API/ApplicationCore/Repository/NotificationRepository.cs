using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public class NotificationRepository : UpDiddyRepositoryBase<Notification>, INotificationRepository
    {
        public NotificationRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
