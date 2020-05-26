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

        public async Task<List<SubscriberEducationHistory>> GetEducationalHistoryBySubscriberGuid(Guid subscriberGuid)
        {
            var educationalHistory = await _dbContext.SubscriberEducationHistory
                                        .Where(seh => seh.Subscriber.SubscriberGuid == subscriberGuid &&
                                                      seh.Subscriber.IsDeleted == 0 &&
                                                      seh.IsDeleted == 0)
                                        .Include(seh => seh.EducationalDegree)
                                        .Include(seh => seh.EducationalInstitution)
                                        .Include(seh => seh.EducationalDegreeType)
                                        .ToListAsync();

            return educationalHistory;

        }
    }
}
