using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class ContactFactory
    {


        public static Contact GetContactByEmail(UpDiddyDbContext db, string email)
        {
            return db.Contact
                .Where(s => s.IsDeleted == 0 && s.Email == email.Trim() )
                .FirstOrDefault();
        }

        public static void AssociateSubscriber(UpDiddyDbContext db, string email, int subscriberId)
        {
            Contact c = GetContactByEmail(db, email);
            if ( c?.SubscriberId != subscriberId)
            {
                c.SubscriberId = subscriberId;
                db.SaveChanges();
            }
        }

    }
}
