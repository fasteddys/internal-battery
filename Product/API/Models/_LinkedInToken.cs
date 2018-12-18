using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class LinkedInToken : BaseModel
    {

        public LinkedInToken()
        {
                
        }

        public LinkedInToken(UpDiddyDbContext db, Guid subscriberGuid)
        {
            var Subscriber = db.Subscriber
                 .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                 .FirstOrDefault();

            if (Subscriber == null)
                throw new Exception($"LinkedInInterface:AcquireBearerToken Subscriber not found for {subscriberGuid}");
        
            this.CreateDate = DateTime.Now;
            this.CreateGuid = Guid.NewGuid();
            this.ModifyDate = DateTime.Now;
            this.ModifyGuid = Guid.NewGuid();
            this.SubscriberId = Subscriber.SubscriberId;
            this.IsDeleted = 0;
        }

    

        public void SetToken(string accessToken, long expires_in_seconds)
        {
            this.ModifyDate = DateTime.Now;
            this.AccessToken = accessToken;
            this.AccessTokenExpiry = DateTime.Now.AddSeconds(expires_in_seconds);

        }


        #region Factory Methods


        public static LinkedInToken GetBySubcriber(UpDiddyDbContext db, Guid subscriberGuid)
        {
            LinkedInToken lit = db.LinkedInToken
            .Include(l => l.Subscriber)
            .Where(l => l.IsDeleted == 0 && l.Subscriber.SubscriberGuid == subscriberGuid)
            .FirstOrDefault();

            return lit;
        }


        public static void StoreToken(UpDiddyDbContext db, LinkedInToken lit , Guid subscriberGuid, string accessToken, long expires_in_seconds)
        {
            bool litExists = (lit != null);
            // Create the lit token if it doesen't exist 
            if (!litExists)
                lit = new LinkedInToken(db, subscriberGuid);
            // Update the user token
            lit.SetToken(accessToken, expires_in_seconds);
            // Save lit token 
            if (!litExists)
                db.LinkedInToken.Add(lit);
            db.SaveChanges();
        }
        #endregion


    }
}
