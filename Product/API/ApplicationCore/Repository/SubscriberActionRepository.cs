using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Data.SqlClient;


namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberActionRepository : UpDiddyRepositoryBase<SubscriberAction>, ISubscriberActionRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberActionRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateSubscriberAction(SubscriberAction subscriberAction)
        {
            await Create(subscriberAction);
            await SaveAsync();
        }

        public async Task<List<SubscriberAction>> GetSubscriberActionByEntityAndEntityType(int entityTypeId, int? entityId = null)
        {
            IEnumerable<SubscriberAction> subscriberActionsList;
            if (entityId != null)
                subscriberActionsList = await GetByConditionAsync(sa => sa.EntityId == entityId && sa.EntityTypeId == entityTypeId && sa.IsDeleted == 0);
            else
                subscriberActionsList = await GetByConditionAsync(sa => sa.EntityTypeId == entityTypeId && sa.IsDeleted == 0);

            return subscriberActionsList.ToList();
        }

        public async Task<List<JobAbandonmentStatistics>> GetJobAbandonmentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var spParams = new object[] {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
                };
            return await _dbContext.JobAbandonmentStatistics.FromSql<JobAbandonmentStatistics>("System_JobAbandonmentStatistics @StartDate, @EndDate", spParams).ToListAsync();
        }
    }
}
