using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class ExperienceLevelFactory
    {
        static public ExperienceLevel GetExperienceLevelByGuid(UpDiddyDbContext db, Guid ExperienceLevelGuid)
        {
            ExperienceLevel experienceLevel = db.ExperienceLevel
           .Where(c => c.IsDeleted == 0 && c.ExperienceLevelGuid == ExperienceLevelGuid)
           .FirstOrDefault();
            return experienceLevel;
        }

    }
}
