using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EducationLevelFactory
    {
        static public async Task<EducationLevel> GetEducationLevelByGuid(IRepositoryWrapper repositoryWrapper, Guid EducationLevelGuid)
        {
             EducationLevel educationLevel = await repositoryWrapper.EducationLevelRepository.GetAll()
            .Where(c => c.IsDeleted == 0 && c.EducationLevelGuid == EducationLevelGuid)
            .FirstOrDefaultAsync();
            return educationLevel;
        }
    
    }
}
