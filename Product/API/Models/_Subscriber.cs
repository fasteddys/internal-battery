using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Subscriber
    {
        #region Factory Methods 

        public static Subscriber GetSubscriberById(UpDiddyDbContext db, int subscriberId)
        {
            return db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .FirstOrDefault();
        }

        #endregion
    }
}
