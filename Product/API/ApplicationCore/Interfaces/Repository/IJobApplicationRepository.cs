using System.Linq;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobApplicationRepository : IUpDiddyRepositoryBase<JobApplication>
    {
        IQueryable<JobApplication> GetAllJobApplicationsAsync();
    }
}
