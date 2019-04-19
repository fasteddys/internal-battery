using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EducationLevelFactory
    {
      
        static public EducationLevel GetEducationLevelByGuid(UpDiddyDbContext db, Guid EducationLevelGuid)
        {
             EducationLevel educationLevel = db.EducationLevel
            .Where(c => c.IsDeleted == 0 && c.EducationLevelGuid == EducationLevelGuid)
            .FirstOrDefault();
            return educationLevel;
        }
    
    }
}
