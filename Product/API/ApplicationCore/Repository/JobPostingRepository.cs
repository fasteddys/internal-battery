using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobPostingRepository :UpDiddyRepositoryBase<JobPosting>, IJobPostingRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public JobPostingRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IQueryable<JobPosting>> GetAllJobPostings()
        {
           return GetAllAsync();
        }
    }
}
