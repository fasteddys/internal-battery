using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class OfferRepository : UpDiddyRepositoryBase<Offer>, IOfferRepository
    {
        public OfferRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Offer> GetOfferByOfferGuid(Guid? offerGuid)
        {
            var offerResult = await GetByConditionAsync(o => o.OfferGuid == offerGuid && o.IsDeleted == 0);
            return offerResult.FirstOrDefault();
        }
    }
}
