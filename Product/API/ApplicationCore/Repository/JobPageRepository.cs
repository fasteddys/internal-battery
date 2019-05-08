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

        public async Task<IEnumerable<JobPage>> GetActiveJobPagesForJobSiteAsync(Guid jobSiteGuid)
        {
            var jobPages = GetByConditionAsync(e => e.JobSite.JobSiteGuid == jobSiteGuid && e.JobPageStatus.Name == "Active" );
            return await jobPages;
        }

        public async Task<JobPage> GetJobPageByJobSiteAndIdentifier(Guid jobSiteGuid, string uniqueIdentifier)
        {
            var jobPage = GetByConditionAsync(e => e.JobSite.JobSiteGuid == jobSiteGuid && e.UniqueIdentifier == uniqueIdentifier).Result.FirstOrDefault();
            return jobPage;
        }
    }
}
