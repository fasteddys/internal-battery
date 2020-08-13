using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberVideoRepository : UpDiddyRepositoryBase<SubscriberVideo>, ISubscriberVideoRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public SubscriberVideoRepository(UpDiddyDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SubscriberVideo> GetExistingSubscriberVideo(Guid subscriberGuid)
            => await GetAll()
                .Include(v => v.Subscriber)
                .SingleOrDefaultAsync(v => v.IsDeleted == 0 && v.Subscriber.IsDeleted == 0 && v.Subscriber.SubscriberGuid == subscriberGuid);

        public async Task<SubscriberVideo> GetExistingOrCreateNewSubscriberVideo(Guid subscriberGuid)
        {
            var subscriberVideo = await GetExistingSubscriberVideo(subscriberGuid);

            if (subscriberVideo != null) { return subscriberVideo; }

            var subscriber = await _dbContext.Subscriber
                .SingleOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid);

            if (subscriber == null)
            {
                throw new NotAuthorizedException($"No subscriber found for \"{subscriberGuid}\"");
            }

            subscriberVideo = new SubscriberVideo
            {
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.NewGuid(),
                IsDeleted = 0,
                Subscriber = subscriber,
                SubscriberVideoGuid = Guid.NewGuid()
            };

            _dbContext.Entry(subscriberVideo).State = EntityState.Added;
            await base.SaveAsync();

            return subscriberVideo;
        }
    }
}
