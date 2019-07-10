using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore.Extensions;

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
            var subscriberResult =  _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.Email == email)
                              .FirstOrDefault();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid)
        {

   
            var subscriberResult = _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefault();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberByIdAsync(int subscriberId)
        {
            return _dbContext.Subscriber
              .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
              .Include( s => s.State)
              .FirstOrDefault(); 
        }

        public async Task<List<Subscriber>> GetSubscribersToIndexIntoGoogle(int subscriberId)
        {
            return _dbContext.Subscriber
              .Where(s => s.IsDeleted == 0 && s.CloudTalentIndexStatus == 0)
              .ToList();
        }

        public async Task<bool> ForceProfileReindex()
        {
            int numUpdates = _dbContext.Database
              .ExecuteSqlCommand("update subscriber set CloudTalentIndexStatus = 0 where isdeleted = 0");             

              return true;
        }







    }
}
