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

        public JobPostingAlertRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<JobPostingAlert>> GetAllJobPostingAlertsBySubscriber(Guid subscriberGuid)
        {
            var jobPostingAlerts = GetAll();

            return await jobPostingAlerts
                .Include(a => a.Subscriber)
                .Where(a => a.Subscriber.SubscriberGuid == subscriberGuid && a.IsDeleted == 0)
                .ToListAsync();
        }

        public async Task<JobPostingAlert> GetJobPostingAlert(Guid jobPostingAlertGuid)
        {
            var jobPostingAlerts = GetAll();
            return await jobPostingAlerts
                .Include(a => a.Subscriber)
                .Where(a => a.JobPostingAlertGuid == jobPostingAlertGuid)
                .FirstOrDefaultAsync();
        }

        public async Task<JobPostingAlert> GetJobPostingAlertBySubscriberGuidAndJobPostingAlertGuid(Guid subscriberGuid, Guid jobPostingAlertGuid)
        {
            return await (from jpa in _dbContext.JobPostingAlert
                          join s in _dbContext.Subscriber on jpa.SubscriberId equals s.SubscriberId
                          where s.SubscriberGuid == subscriberGuid && jpa.JobPostingAlertGuid == jobPostingAlertGuid
                          select jpa).FirstOrDefaultAsync();
        }
    }
}
