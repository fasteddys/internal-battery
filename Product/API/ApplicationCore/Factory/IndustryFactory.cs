using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class IndustryFactory
    {
        
        static public Industry GetIndustryByGuid(UpDiddyDbContext db, Guid IndustryGuid)
        {

            Industry industry = db.Industry
                .Where(c => c.IsDeleted == 0 && c.IndustryGuid == IndustryGuid)
                .FirstOrDefault();
            return industry;
        }
       
    }
}
