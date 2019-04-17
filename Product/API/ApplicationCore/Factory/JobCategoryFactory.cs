using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobCategoryFactory
    {
        static public JobCategory GetJobCategoryByGuid(UpDiddyDbContext db, Guid jobCategoryGuid)
        {

            JobCategory jobCategory = db.JobCategory
                .Where(c => c.IsDeleted == 0 && c.JobCategoryGuid == jobCategoryGuid)
                .FirstOrDefault();
            return jobCategory;
        }
    }
}
