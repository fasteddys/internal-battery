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
    public class CommunityGroupSubscriberRepository : UpDiddyRepositoryBase<CommunityGroupSubscriber>, ICommunityGroupSubscriberRepository
    {
        private readonly UpDiddyDbContext _dbContext;
       // private readonly ICommunityGroupSubscriberRepository _communityGroupSubscriberRepository;
        public CommunityGroupSubscriberRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
           // _communityGroupSubscriberRepository = communityGroupSubscriberRepository;
            _dbContext = dbContext;
        }


        public IQueryable<Subscriber> GetAllCommunityGroupSubscribers(int CommunityGroupId)
        {
            return  _dbContext.CommunityGroupSubscriber
                              .Where(s => s.CommunityGroupId == CommunityGroupId)
                              .Select(s => s.Subscriber);
        }

        public IQueryable<Subscriber> GetAllCommunityGroupSubscribers(Guid CommunityGroupGuid)
        {
            return  _dbContext.CommunityGroupSubscriber
                              .Where(s => s.CommunityGroup.CommunityGroupGuid == CommunityGroupGuid)
                              .Select(s => s.Subscriber);
        }

        public async Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberByLastName(int CommunityGroupId, string name)
        {
            return await _dbContext.CommunityGroupSubscriber
                              .Where(s => s.CommunityGroupId == CommunityGroupId && s.Subscriber.LastName == name)
                              .FirstOrDefaultAsync();
        }

        public async Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberByLastName(Guid CommunityGroupGuid, string name)
        {
            return await _dbContext.CommunityGroupSubscriber
                              .Where(s => s.CommunityGroupSubscriberGuid == CommunityGroupGuid && s.Subscriber.LastName == name)
                              .FirstOrDefaultAsync();
        }

        public async Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberById(int CommunityGroupSubscriberId)
        {
            return await _dbContext.CommunityGroupSubscriber
                              .Where(s => s.CommunityGroupSubscriberId == CommunityGroupSubscriberId)
                              .FirstOrDefaultAsync();

        }

        public async Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberByGuid(Guid CommunityGroupSubscriberGuid)
        {
            var communityGroupResult = _dbContext.CommunityGroupSubscriber
                              .Where(s => s.CommunityGroupSubscriberGuid == CommunityGroupSubscriberGuid)
                              .FirstOrDefault();

            return communityGroupResult;
        }




    }
}
