using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public class PartnerContactRepository : UpDiddyRepositoryBase<PartnerContact>, IPartnerContactRepository
    {
        UpDiddyDbContext _dbContext;
        public PartnerContactRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<Partner>> GetPartnersAssociatedWithSubscriber(int SubscriberId)
        {
            Contact contact = _dbContext.Contact.Where(c => c.SubscriberId == SubscriberId).FirstOrDefault();

            // If user was not in contacts table prior to signing up, they're not associated with external partner
            // in our system, so return null.
            if (contact == null)
                return null;

            IList<Partner> Partners = new List<Partner>();
            foreach(PartnerContact partnerContact in _dbContext.PartnerContact.Where(pc => pc.ContactId == contact.ContactId).Include(pc => pc.Partner))
            {
                Partners.Add(partnerContact.Partner);
            }
            return Partners;
        }
    }
}
