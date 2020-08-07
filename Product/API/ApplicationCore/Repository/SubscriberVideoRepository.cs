using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberVideoRepository : UpDiddyRepositoryBase<SubscriberVideo>, ISubscriberVideoRepository
    {
        public SubscriberVideoRepository(UpDiddyDbContext dbContext)
            : base(dbContext)
        { }

        public async Task<List<SubscriberVideo>> GetSubscriberVideos(Guid subscriberGuid)
            => await GetAllWithTracking()
                .Include(v => v.Subscriber)
                .Where(v => v.Subscriber.SubscriberGuid == subscriberGuid && v.IsDeleted == 0)
                .ToListAsync();

        public async Task<SubscriberVideo> GetSubscriberVideo(Guid subscriberVideoGuid, Guid subscriberGuid)
            => await GetAllWithTracking()
                .Include(v => v.Subscriber)
                .SingleOrDefaultAsync(v => v.SubscriberVideoGuid == subscriberVideoGuid && v.Subscriber.SubscriberGuid == subscriberGuid && v.IsDeleted == 0);
    }
}
