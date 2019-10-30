using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SecurityClearanceFactory
    {
        static public async Task<SecurityClearance> GetSecurityClearanceByGuid(IRepositoryWrapper repositoryWrapper, Guid SecurityClearanceGuid)
        {
            SecurityClearance securityClearance = await repositoryWrapper.SecurityClearanceRepository.GetAll()
                .Where(c => c.IsDeleted == 0 && c.SecurityClearanceGuid == SecurityClearanceGuid)
                .FirstOrDefaultAsync();
            return securityClearance;
        }

    }
}
