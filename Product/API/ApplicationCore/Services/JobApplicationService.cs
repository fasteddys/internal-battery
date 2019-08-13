using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public JobApplicationService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<bool> IsSubscriberAppliedToJobPosting(int subscriberId, int jobPostingId)
        {
            IQueryable<JobApplication> jobPosting = _repositoryWrapper.JobApplication.GetAll();
            return await jobPosting.AnyAsync(x => x.SubscriberId == subscriberId && x.JobPostingId == jobPostingId);
        }     
    }
}