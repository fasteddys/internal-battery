using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class ExperienceLevelFactory
    {
        static public async Task<ExperienceLevel> GetExperienceLevelByGuid(IRepositoryWrapper repositoryWrapper, Guid ExperienceLevelGuid)
        {
            ExperienceLevel experienceLevel = await repositoryWrapper.ExperienceLevelRepository.GetAll()
           .Where(c => c.IsDeleted == 0 && c.ExperienceLevelGuid == ExperienceLevelGuid)
           .FirstOrDefaultAsync();
            return experienceLevel;
        }

    }
}
