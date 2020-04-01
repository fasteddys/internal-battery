using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberFileRepository : UpDiddyRepositoryBase<SubscriberFile>, ISubscriberFileRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberFileRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<SubscriberFile> GetAllSubscriberFileQueryableAsync()
        {
            return GetAll();
        }

        public async Task UpdateSubscriberFileAsync(SubscriberFile subscriberFile)
        {
            Update(subscriberFile);
            await SaveAsync();
        }

        public async Task<List<SubscriberFile>> GetAllSubscriberFilesBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from sf in _dbContext.SubscriberFile
                          join s in _dbContext.Subscriber on sf.SubscriberId equals s.SubscriberId
                          where s.SubscriberGuid == subscriberGuid && sf.IsDeleted == 0
                          select sf).ToListAsync();
        }

        public async Task<SubscriberFile> GetMostRecentBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from sf in _dbContext.SubscriberFile
                          join s in _dbContext.Subscriber on sf.SubscriberId equals s.SubscriberId
                          where s.SubscriberGuid == subscriberGuid && sf.IsDeleted == 0
                          orderby sf.CreateDate descending
                          select sf).FirstOrDefaultAsync();
        }
    }
}
