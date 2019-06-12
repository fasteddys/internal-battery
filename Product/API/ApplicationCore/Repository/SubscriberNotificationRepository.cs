using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public class SubscriberNotificationRepository : UpDiddyRepositoryBase<SubscriberNotification>, ISubscriberNotificationRepository
    {
        public SubscriberNotificationRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
