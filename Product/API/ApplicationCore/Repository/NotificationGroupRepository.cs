using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class NotificationGroupRepository : UpDiddyRepositoryBase<NotificationGroup>, INotificationGroupRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public NotificationGroupRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


    }
}
