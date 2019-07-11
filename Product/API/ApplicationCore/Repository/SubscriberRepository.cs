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


        /*
        public async Task<List<Subscriber>> GetSubscribersToIndexIntoGoogle(int numSubscribers, int indexVersion)
        {


            var queryableSubscriber = await GetAllAsync();
            var rVal = queryableSubscriber
              .Where(s => s.IsDeleted == 0 && s.CloudTalentIndexVersion < indexVersion)
              .Take(numSubscribers)
              .ToList();

            if ( rVal.Count > 0 )
            {
                // build sql to update subscribers who will be updated 
                string updateSql = $"update subscriber set CloudTalentIndexVersion = {indexVersion} where subscriberid in (";
                string inList = string.Empty;
                foreach (Subscriber s in rVal)
                {
                    if (string.IsNullOrEmpty(inList) == false)
                        inList += ",";
                    inList += s.SubscriberId.ToString();

                }
                updateSql += inList + ")"; 
                ExecuteSQL(updateSql);

            }

            return rVal;

        }
        */

    







    }
}
