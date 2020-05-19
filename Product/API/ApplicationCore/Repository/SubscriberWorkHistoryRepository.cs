using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.B2B;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberWorkHistoryRepository : UpDiddyRepositoryBase<SubscriberWorkHistory>, ISubscriberWorkHistoryRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public SubscriberWorkHistoryRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SubscriberWorkHistory> GetLastEmploymentDetailBySubscriberGuid(Guid subscriberGuid)
        {
            var workHistory = await _dbContext.SubscriberWorkHistory
                                    .Where(swh => swh.Subscriber.SubscriberGuid == subscriberGuid &&
                                                  swh.Subscriber.IsDeleted == 0 &&
                                                  swh.IsDeleted == 0)
                                    .ToListAsync();
            
            if(workHistory != null && workHistory.Count > 0)
            {
                var maxStartDate = workHistory.Max(sd => sd.StartDate);
                var maxEndDate = workHistory.Max(sd => sd.EndDate);

                if(maxStartDate.HasValue && maxEndDate.HasValue && maxStartDate.Value >= maxEndDate.Value)
                {
                    return workHistory.FirstOrDefault(wh => wh.StartDate == maxStartDate);
                }
                else if (maxStartDate.HasValue && maxEndDate.HasValue && maxStartDate.Value < maxEndDate.Value)
                {
                    return workHistory.FirstOrDefault(wh => wh.EndDate == maxEndDate);
                }
                else if(maxStartDate.HasValue && !maxEndDate.HasValue)
                {
                    return workHistory.FirstOrDefault(wh => wh.StartDate == maxStartDate);
                }
                else if(!maxStartDate.HasValue && maxEndDate.HasValue)
                {
                    return workHistory.FirstOrDefault(wh => wh.EndDate == maxEndDate);
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }

        }

        public async Task<List<SubscriberWorkHistory>> GetWorkHistoryBySubscriberGuid(Guid subscriberGuid)
        {
            var workHistory = await _dbContext.SubscriberWorkHistory
                        .Where(swh => swh.Subscriber.SubscriberGuid == subscriberGuid &&
                                      swh.Subscriber.IsDeleted == 0 &&
                                      swh.IsDeleted == 0 )
                        .ToListAsync();

            return workHistory;
        }

    }
}
