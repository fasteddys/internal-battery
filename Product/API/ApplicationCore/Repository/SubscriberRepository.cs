using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberRepository : UpDiddyRepositoryBase<Subscriber>, ISubscriberRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IQueryable<Subscriber>> GetAllSubscribersAsync()
        {
            return GetAllAsync();
        }

        public async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            var queryableSubscriber = await GetAllAsync();

            var subscriberResult = queryableSubscriber
                              .Where(s => s.IsDeleted == 0 && s.Email == email)
                              .ToList();

            return subscriberResult.Count == 0 ? null : subscriberResult[0];
        }

        public async Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid)
        {
            var queryableSubscriber = await GetAllAsync();

            var subscriberResult = queryableSubscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .ToList();

            return subscriberResult.Count == 0 ? null : subscriberResult[0];
        }

        public async Task<Subscriber> GetSubscriberByIdAsync(int subscriberId)
        {
            return _dbContext.Subscriber
              .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
              .FirstOrDefault(); 
        }



      

    }
}
