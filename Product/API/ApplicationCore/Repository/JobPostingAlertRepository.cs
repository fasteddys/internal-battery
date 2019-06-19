using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobPostingAlertRepository : UpDiddyRepositoryBase<JobPostingAlert>, IJobPostingAlertRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public JobPostingAlertRepository(UpDiddyDbContext dbContext): base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<JobPostingAlert>> GetAllJobPostingAlertsBySubscriber(Guid subscriberGuid)
        {
            var jobPostingAlerts = GetAllAsync();

            return await jobPostingAlerts.Result
                .Include(a => a.Subscriber)
                .Where(a => a.Subscriber.SubscriberGuid == subscriberGuid)
                .ToListAsync();
        }

        public async Task<JobPostingAlert> GetJobPostingAlert(Guid jobPostingAlertGuid)
        {
            var jobPostingAlerts = GetAllAsync();
            return await jobPostingAlerts.Result
                .Include(a => a.Subscriber) 
                .Where(a => a.JobPostingAlertGuid == jobPostingAlertGuid)
                .FirstOrDefaultAsync();
        }
    }
}
