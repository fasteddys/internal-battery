using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class TalentFavoriteRepository : UpDiddyRepositoryBase<TalentFavorite>, ITalentFavoriteRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public TalentFavoriteRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<TalentFavorite> GetBySubscriberGuidAndTalentGuid(Guid subscriberGuid, Guid talentGuid)
        {
            return await (from cf in _dbContext.TalentFavorite
                          join s in _dbContext.Subscriber on cf.SubscriberId equals s.SubscriberId
                          join t in _dbContext.Subscriber on cf.TalentId equals t.SubscriberId
                          where s.SubscriberGuid == subscriberGuid && t.SubscriberGuid == talentGuid && cf.IsDeleted == 0
                          select cf).FirstOrDefaultAsync();
        }


        public async Task<List<TalentFavorite>> GetTalentForSubscriber(Guid subscriberGuid, int limit = 30, int offset = 0 , string sort = "CreateDate"  , string order = "descending")
        {

            PropertyInfo sortProp = typeof(TalentFavorite).GetProperty(sort, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            List<TalentFavorite> entity = new List<TalentFavorite>();
            switch (order)
            {
                case "descending":
                    entity = await _dbContext.TalentFavorite
                    .Include(t => t.Talent)
                    .AsNoTracking()
                    .OrderByDescending(x => sortProp.GetValue(x, null))
                    .Where(s => s.Subscriber.SubscriberGuid == subscriberGuid && s.IsDeleted == 0 )
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
                    break;
                default:
                    entity = await _dbContext.TalentFavorite
                    .Include(t => t.Talent)
                    .AsNoTracking()
                    .OrderBy(x => sortProp.GetValue(x, null))
                    .Where(s => s.Subscriber.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
                    break;
            }

            return entity; 
              
        }
    }
}
