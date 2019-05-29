using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public class JobCategoryRepository : UpDiddyRepositoryBase<JobCategory>, IJobCategoryRepository
    {
        public JobCategoryRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
