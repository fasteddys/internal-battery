using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobReferralRepository : UpDiddyRepositoryBase<JobReferral>, IJobReferralRepository
    {

        private readonly UpDiddyDbContext _dbContext;
        public JobReferralRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Guid> AddJobReferralAsync(JobReferral jobReferral)
        {
            //persist jobReferral to database and return jobReferralGuid
            await Create(jobReferral);
            SaveAsync().Wait();

            return jobReferral.JobReferralGuid;
        }

        public async Task<JobReferral> GetJobReferralByGuid(Guid jobReferralGuid)
        {
            var queryableJobReferral = GetAll();
            var jobReferralResult = await queryableJobReferral
                                .Where(jr => jr.IsDeleted == 0 && jr.JobReferralGuid == jobReferralGuid)
                                .ToListAsync();

            return jobReferralResult.Count == 0 ? null : jobReferralResult[0];
        }

        public async Task UpdateJobReferral(JobReferral jobReferral)
        {
            //persist jobReferral changes to database 
            Update(jobReferral);
            await SaveAsync();
        }
    }
}
