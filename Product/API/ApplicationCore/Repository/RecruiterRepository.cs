using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class RecruiterRepository : UpDiddyRepositoryBase<Recruiter>, IRecruiterRepository
    {
        public RecruiterRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }

        public async Task<IQueryable<Recruiter>> GetAllRecruiters()
        {
            return await GetAllAsync();
        }

        public async Task<Recruiter> GetRecruiterBySubscriberId(int subscriberId)
        {
            var queryableRecruiter = await GetAllAsync();
            var recruiterResult = queryableRecruiter
                                .Where(jr => jr.IsDeleted == 0 && jr.SubscriberId == subscriberId)
                                .ToList();

            return recruiterResult.Count == 0 ? null : recruiterResult[0];
        }
    }
}
