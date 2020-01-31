using System.Linq;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobApplicationRepository :UpDiddyRepositoryBase<JobApplication>, IJobApplicationRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public JobApplicationRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<JobApplication> GetAllJobApplicationsAsync()
        {
           return GetAll();
        }

        public async Task<bool> HasSubscriberAppliedToJobPosting(Guid subscriberGuid, Guid jobPostingGuid)
        {
            return await (from a in _dbContext.JobApplication
                          join b in _dbContext.JobPosting on a.JobPostingId equals b.JobPostingId
                          join c in _dbContext.Subscriber on a.SubscriberId equals c.SubscriberId
                          where b.JobPostingGuid == jobPostingGuid && c.SubscriberGuid.Value == subscriberGuid 
                          select a
                    ).AnyAsync();
        }
    }
}
