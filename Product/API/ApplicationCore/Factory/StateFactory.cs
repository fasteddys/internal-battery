using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class StateFactory
    {
 
        public static State GetStateByStateCode(UpDiddyDbContext db, string stateCode)
        {
            return db.State
                .Where(s => s.IsDeleted == 0 && s.Code == stateCode.Trim())
                .FirstOrDefault();
        }
    }
}
