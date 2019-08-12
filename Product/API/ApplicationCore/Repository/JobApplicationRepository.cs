using System.Linq;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

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
    }
}
