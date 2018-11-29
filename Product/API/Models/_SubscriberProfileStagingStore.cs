using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddy.Helpers;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Models
{
    public partial class SubscriberProfileStagingStore : BaseModel
    {
        public SubscriberProfileStagingStore()
        {
                
        }

        public SubscriberProfileStagingStore(UpDiddyDbContext db, Guid subscriberGuid)
        {
            var Subscriber = db.Subscriber
            .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
              .FirstOrDefault();

            if (Subscriber == null)
                throw new Exception($"LinkedInInterface:ImportProfile Subscriber not found for {subscriberGuid}");
 
            this.CreateDate = DateTime.Now;
            this.ModifyGuid = Guid.NewGuid();
            this.SubscriberId = Subscriber.SubscriberId;
            this.ProfileSource = Constants.LinkedInProfile;
            this.IsDeleted = 0;
            this.ProfileFormat = Constants.DataFormatJson;

        }

        #region Factory Methods 

        public static SubscriberProfileStagingStore GetBySubcriber(UpDiddyDbContext db, Guid subscriberGuid)
        {
            SubscriberProfileStagingStore pss = db.SubscriberProfileStagingStore
                 .Include(s => s.Subscriber)
                 .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid && s.ProfileSource == Constants.LinkedInProfile)
             .FirstOrDefault();

            return pss;
        }


        public static void StoreProfileData(UpDiddyDbContext db, Guid subscriberGuid, string ResponseJson )
        {
            SubscriberProfileStagingStore pss = new SubscriberProfileStagingStore(db, subscriberGuid);

            pss.ModifyDate = DateTime.Now;
            pss.ProfileData = ResponseJson;
            pss.Status = (int)ProfileDataStatus.Acquired;
            
            db.SubscriberProfileStagingStore.Add(pss);
            db.SaveChanges();
        }

        #endregion



    }
}
