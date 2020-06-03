using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class StateFactory
    {
        public static async Task<State> GetStateByStateCode(IRepositoryWrapper repositoryWrapper, string stateCode, string countryCode = "US")
        {
            return await repositoryWrapper.State.GetAllWithTracking()
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.IsDeleted == 0 && s.Country.Code2 == countryCode && s.Code == stateCode.Trim());
        }
    }
}
