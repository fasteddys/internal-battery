using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobPostingRepository : UpDiddyRepositoryBase<JobPosting>, IJobPostingRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public JobPostingRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<JobPosting> GetAllJobPostings()
        {
            return GetAll();
        }

        public async Task<JobPosting> GetJobPostingById(int id)
        {
            return await (from a in _dbContext.JobPosting
                          where a.JobPostingId == id
                          select a).FirstOrDefaultAsync();
        }

        public async Task<JobPosting> GetJobPostingByGuid(Guid jobPostingGuid)
        {

            var queryableJobPosting = GetAll();
            var jobPostingResult = await queryableJobPosting
                                .Where(jp => jp.IsDeleted == 0 && jp.JobPostingGuid == jobPostingGuid)
                                .ToListAsync();

            return jobPostingResult.Count == 0 ? null : jobPostingResult[0];
        }

        public async Task<string> GetCloudTalentUri(Guid jobPostingGuid)
        {
            return await (from jobPosting in _dbContext.JobPosting
                          where jobPosting.JobPostingGuid == jobPostingGuid
                          select jobPosting.CloudTalentUri).FirstOrDefaultAsync();
        }
    }
}
