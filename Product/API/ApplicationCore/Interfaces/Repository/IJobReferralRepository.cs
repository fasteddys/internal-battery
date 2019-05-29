using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobReferralRepository : IUpDiddyRepositoryBase<JobReferral>
    {
        Task<Guid> AddJobReferralAsync(JobReferral jobReferral);
        Task<JobReferral> GetJobReferralByGuid(Guid jobReferralGuid);
        Task UpdateJobReferral(JobReferral jobReferral);
    }
}
