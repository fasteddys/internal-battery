using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.B2B;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberEducationHistoryRepository : UpDiddyRepositoryBase<SubscriberEducationHistory>, ISubscriberEducationHistoryRepository
    {

        private readonly UpDiddyDbContext _dbContext;

        public SubscriberEducationHistoryRepository(UpDiddyDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<List<SubscriberEducationHistory>> GetEducationalHistoryBySubscriberGuid(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var educationalHistory = _dbContext.SubscriberEducationHistory
                                        .Where(seh => seh.Subscriber.SubscriberGuid == subscriberGuid &&
                                                      seh.Subscriber.IsDeleted == 0 &&
                                                      seh.IsDeleted == 0)
                                        .Include(seh => seh.EducationalDegree)
                                        .Include(seh => seh.EducationalInstitution)
                                        .Include(seh => seh.EducationalDegreeType)
                                        .Skip(limit*offset)
                                        .Take(limit);

            //sorting            
            if(order.ToLower() == "descending")
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        educationalHistory = educationalHistory.OrderByDescending(s => s.ModifyDate);
                        break;
                    case "createdate":
                        educationalHistory = educationalHistory.OrderByDescending(s => s.CreateDate);
                        break;
                    default:
                        educationalHistory = educationalHistory.OrderByDescending(s => s.ModifyDate);
                        break;
                }
            }
            else
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        educationalHistory = educationalHistory.OrderBy(s => s.ModifyDate);
                        break;
                    case "createdate":
                        educationalHistory = educationalHistory.OrderBy(s => s.CreateDate);
                        break;
                    default:
                        educationalHistory = educationalHistory.OrderBy(s => s.ModifyDate);
                        break;
                }
            }


            return await educationalHistory.ToListAsync();

        }
    }
}
