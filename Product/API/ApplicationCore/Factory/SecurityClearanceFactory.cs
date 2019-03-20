using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SecurityClearanceFactory
    {
        static public SecurityClearance GetSecurityClearanceByGuid(UpDiddyDbContext db, Guid SecurityClearanceGuid)
        {

            SecurityClearance securityClearance = db.SecurityClearance
                .Where(c => c.IsDeleted == 0 && c.SecurityClearanceGuid == SecurityClearanceGuid)
                .FirstOrDefault();
            return securityClearance;
        }

    }
}
