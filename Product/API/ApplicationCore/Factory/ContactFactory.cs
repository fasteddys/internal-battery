using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

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

        static public async Task<Contact> GetOrAdd(IRepositoryWrapper repositoryWrapper, string email, string firstName, string lastName, Subscriber subscriber)
        {
            email = email.Trim();
            Contact contact =  await repositoryWrapper.ContactRepository.GetAllWithTracking()
                .Where(c => c.IsDeleted == 0 && c.Email == email)
                .FirstOrDefaultAsync();

            if (contact == null)
            {
                contact = CreateContact(email, firstName, lastName, subscriber);
                await repositoryWrapper.ContactRepository.Create(contact);
                await repositoryWrapper.ContactRepository.SaveAsync();
            }
            return contact;
        }

        public static async Task<Contact> GetContactByEmail(IRepositoryWrapper repositoryWrapper, string email)
        {
            return await repositoryWrapper.ContactRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.Email == email.Trim() )
                .FirstOrDefaultAsync();
        }

        public static async Task AssociateSubscriber(IRepositoryWrapper repositoryWrapper, string email, int subscriberId)
        {
            Contact c = await GetContactByEmail(repositoryWrapper, email );

            if ( c != null && c?.SubscriberId != subscriberId)
            {
                c.SubscriberId = subscriberId;
                await repositoryWrapper.ContactRepository.SaveAsync();
            }
        }

    }
}
