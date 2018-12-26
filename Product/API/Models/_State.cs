using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class State
    {    
        public static State GetStateByStateCode(UpDiddyDbContext db, string stateCode)
        {
            return db.State
                .Where(s => s.IsDeleted == 0 && s.Code == stateCode.Trim())
                .FirstOrDefault();
        }

    }
}
