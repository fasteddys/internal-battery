using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobCategoryFactory
    {
        static public async Task<JobCategory> GetJobCategoryByGuid(IRepositoryWrapper repositoryWrapper, Guid jobCategoryGuid)
        {
            JobCategory jobCategory = await repositoryWrapper.JobCategoryRepository.GetAllWithTracking()
                .Where(c => c.IsDeleted == 0 && c.JobCategoryGuid == jobCategoryGuid)
                .FirstOrDefaultAsync();
            return jobCategory;
        }
    }
}
