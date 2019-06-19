using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class ContactFactory
    {
        static public Contact CreateContact(string email, string firstName, string lastName, Subscriber subscriber)
        {
            Contact rVal = new Contact();
            rVal.Email = email;
            if (subscriber != null && subscriber.SubscriberId > 0)
                rVal.SubscriberId = subscriber.SubscriberId;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.ContactGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public Contact GetOrAdd(UpDiddyDbContext db, string email, string firstName, string lastName, Subscriber subscriber)
        {
            email = email.Trim();
            Contact contact = db.Contact
                .Where(c => c.IsDeleted == 0 && c.Email == email)
                .FirstOrDefault();

            if (contact == null)
            {
                contact = CreateContact(email, firstName, lastName, subscriber);
                db.Contact.Add(contact);
                db.SaveChanges();
            }
            return contact;
        }

        public static Contact GetContactByEmail(UpDiddyDbContext db, string email)
        {
            return db.Contact
                .Where(s => s.IsDeleted == 0 && s.Email == email.Trim() )
                .FirstOrDefault();
        }

        public static void AssociateSubscriber(UpDiddyDbContext db, string email, int subscriberId)
        {
            Contact c = GetContactByEmail(db, email );

            if ( c != null && c?.SubscriberId != subscriberId)
            {
                c.SubscriberId = subscriberId;
                db.SaveChanges();
            }
        }

    }
}
