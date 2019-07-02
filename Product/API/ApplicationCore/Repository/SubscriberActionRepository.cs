using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberActionRepository : UpDiddyRepositoryBase<SubscriberAction>, ISubscriberActionRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberActionRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
