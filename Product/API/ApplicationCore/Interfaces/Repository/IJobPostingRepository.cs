using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobPostingRepository : IUpDiddyRepositoryBase<JobPosting>
    {
        IQueryable<JobPosting> GetAllJobPostings();
        Task<JobPosting> GetJobPostingByGuid(Guid jobPostingGuid);
        Task<JobPosting> GetJobPostingById(int Id);
    }
}
