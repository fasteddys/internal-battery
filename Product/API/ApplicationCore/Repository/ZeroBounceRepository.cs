using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ZeroBounceRepository : UpDiddyRepositoryBase<ZeroBounce>, IZeroBounceRepository
    {
        public ZeroBounceRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }

        public async Task<bool?> MostRecentResultInLast90Days(string email)
        {
            var requests = GetAllAsync();
            var mostRecentResultInLast90Days = requests.Result
                                .Include(zb => zb.PartnerContact).ThenInclude(pc => pc.Contact)
                                .Where(zb => zb.IsDeleted == 0
                                    && zb.PartnerContact.Contact.Email == email 
                                    && EF.Functions.DateDiffDay(zb.CreateDate, DateTime.UtcNow) <= 90)
                                .OrderByDescending(zb => zb.CreateDate)
                                .FirstOrDefault();

            if (mostRecentResultInLast90Days != null)
            {
                var status = mostRecentResultInLast90Days.Response["status"];
                if (status != null)
                        return (status.ToString() == "valid") ? true : false;
            }

            return null;
        }
    }
}
