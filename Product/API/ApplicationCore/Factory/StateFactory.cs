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
 
        public static async Task<State> GetStateByStateCode(IRepositoryWrapper repositoryWrapper, string stateCode)
        {
            return await repositoryWrapper.State.GetAll()
                .Where(s => s.IsDeleted == 0 && s.Code == stateCode.Trim())
                .FirstOrDefaultAsync();
        }
    }
}
