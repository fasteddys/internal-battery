using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobPostingAlertRepository : IUpDiddyRepositoryBase<JobPostingAlert>
    {
        Task<IEnumerable<JobPostingAlert>> GetAllJobPostingAlertsBySubscriber(Guid subscriberGuid);
        Task<JobPostingAlert> GetJobPostingAlert(Guid jobPostingAlertGuid);
    }
}
