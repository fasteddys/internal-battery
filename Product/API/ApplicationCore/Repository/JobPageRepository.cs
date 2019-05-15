using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobPageRepository : UpDiddyRepositoryBase<JobPage>, IJobPageRepository
    {
        public JobPageRepository(UpDiddyDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<JobPage>> GetAllJobPagesForJobSiteAsync(Guid jobSiteGuid)
        {
            var jobPages = GetByConditionAsync(e => e.JobSite.JobSiteGuid == jobSiteGuid);
            return await jobPages;
        }
    }
}
