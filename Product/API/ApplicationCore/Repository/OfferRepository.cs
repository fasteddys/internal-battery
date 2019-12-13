using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class OfferRepository : UpDiddyRepositoryBase<Offer>, IOfferRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public OfferRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Offer> GetOfferByOfferGuid(Guid? offerGuid)
        {
            var offerResult = await GetByConditionAsync(o => o.OfferGuid == offerGuid && o.IsDeleted == 0);
            return offerResult.FirstOrDefault();
        }

        public async Task<List<Offer>> GetAllOffers(int limit, int offset)
        {
            var currentDate = DateTime.UtcNow;
            var offers = await (from o in _dbContext.Offer.Include(x => x.Partner)
                                join p in _dbContext.Partner on o.PartnerId equals p.PartnerId
                                where o.IsDeleted == 0 
                                select o).Skip(offset).Take(limit).ToListAsync();
            return offers;
        }
    }
}
