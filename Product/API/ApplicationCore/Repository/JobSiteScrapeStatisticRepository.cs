using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{

    //     public class JobSiteRepository : UpDiddyRepositoryBase<JobSite>, IJobSiteRepository
    public class JobSiteScrapeStatisticRepository : UpDiddyRepositoryBase<JobSiteScrapeStatistic>, IJobSiteScrapeStatisticRepository
    {        
        private readonly UpDiddyDbContext _dbContext;
        public JobSiteScrapeStatisticRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;

        }

        public async Task<IEnumerable<JobSiteScrapeStatistic>> GetJobScrapeStatisticsAsync(int numRecords )
        {
          return _dbContext.JobSiteScrapeStatistic
                 .Include ( s => s.JobSite )
                 .Where(s => s.IsDeleted == 0)
                 .OrderByDescending(s => s.ScrapeDate)
                 .Take(numRecords)
                 .ToList();    
        }

    }
}
