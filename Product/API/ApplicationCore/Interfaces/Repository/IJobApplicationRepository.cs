using System.Linq;
using UpDiddyApi.Models;
using System;
using System.Threading.Tasks;
namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobApplicationRepository : IUpDiddyRepositoryBase<JobApplication>
    {
        IQueryable<JobApplication> GetAllJobApplicationsAsync();
        Task<bool> HasSubscriberAppliedToJobPosting(Guid subscriberGuid, Guid jobPostingGuid);
    }
}
