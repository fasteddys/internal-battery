using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class IndustryFactory
    {

        static public async Task<Industry> GetIndustryByGuid(IRepositoryWrapper repositoryWrapper, Guid IndustryGuid)
        {

            Industry industry = await repositoryWrapper.IndustryRepository.GetAllWithTracking()
                .Where(c => c.IsDeleted == 0 && c.IndustryGuid == IndustryGuid)
                .FirstOrDefaultAsync();
            return industry;
        }
    }
}
