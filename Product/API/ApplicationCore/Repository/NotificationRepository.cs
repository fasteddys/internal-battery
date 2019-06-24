using Microsoft.EntityFrameworkCore;
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
        UpDiddyDbContext _dbContext;
        public NotificationRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IQueryable<Notification>> GetAllNonDeleted()
        {
            return _dbContext.Set<Notification>().IgnoreQueryFilters().Where(n => n.IsDeleted == 0).AsNoTracking();
        }
    }
}
