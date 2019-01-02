using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Business.Factory
{
    public class LinkedInTokenFactory
    {
        static public LinkedInToken CreateLinkedInToken(UpDiddyDbContext db, Guid subscriberGuid)
        {
            var Subscriber = db.Subscriber
                 .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                 .FirstOrDefault();

            if (Subscriber == null)
                throw new Exception($"LinkedInInterface:AcquireBearerToken Subscriber not found for {subscriberGuid}");

            LinkedInToken rVal = new LinkedInToken();
            rVal.CreateDate = DateTime.Now;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyDate = DateTime.Now;
            rVal.ModifyGuid = Guid.NewGuid();
            rVal.SubscriberId = Subscriber.SubscriberId;
            rVal.IsDeleted = 0;
            return rVal;

        }



        public static void SetToken(LinkedInToken token, string accessToken, long expires_in_seconds)
        {
            token.ModifyDate = DateTime.Now;
            token.AccessToken = accessToken;
            token.AccessTokenExpiry = DateTime.Now.AddSeconds(expires_in_seconds);

        }

 

        public static LinkedInToken GetBySubcriber(UpDiddyDbContext db, Guid subscriberGuid)
        {
            LinkedInToken lit = db.LinkedInToken
            .Include(l => l.Subscriber)
            .Where(l => l.IsDeleted == 0 && l.Subscriber.SubscriberGuid == subscriberGuid)
            .FirstOrDefault();

            return lit;
        }


        public static void StoreToken(UpDiddyDbContext db, LinkedInToken lit, Guid subscriberGuid, string accessToken, long expires_in_seconds)
        {
            bool litExists = (lit != null);
            // Create the lit token if it doesen't exist 
            if (!litExists)
                lit = CreateLinkedInToken(db, subscriberGuid);
            // Update the user token
            SetToken(lit, accessToken, expires_in_seconds);
            // Save lit token 
            if (!litExists)
                db.LinkedInToken.Add(lit);
            db.SaveChanges();
        }
    }
}
