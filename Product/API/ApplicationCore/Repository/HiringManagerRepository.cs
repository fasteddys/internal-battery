using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class HiringManagerRepository: UpDiddyRepositoryBase<HiringManager>, IHiringManagerRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public HiringManagerRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HiringManager> GetHiringManagerBySubscriberId(int SubscriberId)
        {
            var hiringManager = await _dbContext.HiringManager.Where(hm => hm.SubscriberId == SubscriberId)
                .Include(hm => hm.Company)
                .Include(hm => hm.Company.Industry)
                .FirstOrDefaultAsync();

            return hiringManager;
        }
    }
}
