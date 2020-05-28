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

        public async Task<List<SubscriberWorkHistory>> GetWorkHistoryBySubscriberGuid(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var workHistory = _dbContext.SubscriberWorkHistory
                                .Where(swh => swh.Subscriber.SubscriberGuid == subscriberGuid &&
                                              swh.Subscriber.IsDeleted == 0 &&
                                              swh.IsDeleted == 0 )
                                .Include(swh => swh.Company)
                                .Skip(limit * offset)
                                .Take(limit);

            //sorting            
            if (order.ToLower() == "descending")
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        workHistory = workHistory.OrderByDescending(s => s.ModifyDate);
                        break;
                    case "createdate":
                        workHistory = workHistory.OrderByDescending(s => s.CreateDate);
                        break;
                    default:
                        workHistory = workHistory.OrderByDescending(s => s.ModifyDate);
                        break;
                }
            }
            else
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        workHistory = workHistory.OrderBy(s => s.ModifyDate);
                        break;
                    case "createdate":
                        workHistory = workHistory.OrderBy(s => s.CreateDate);
                        break;
                    default:
                        workHistory = workHistory.OrderBy(s => s.ModifyDate);
                        break;
                }
            }

            return await workHistory.ToListAsync();
        }

    }
}
