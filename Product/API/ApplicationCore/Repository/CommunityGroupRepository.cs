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
    public class CommunityGroupRepository : UpDiddyRepositoryBase<CommunityGroup>, ICommunityGroupRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        private readonly ICommunityGroupSubscriberRepository _communityGroupSubscriberRepository;
        public CommunityGroupRepository(UpDiddyDbContext dbContext, ICommunityGroupSubscriberRepository communityGroupSubscriberRepository) : base(dbContext)
        {
            _communityGroupSubscriberRepository = communityGroupSubscriberRepository;
            _dbContext = dbContext;
        }

        public IQueryable<CommunityGroup> GetAllCommunityGroupsAsync()
        {
            return GetAll();
        }

        public async Task<CommunityGroup> GetCommunityGroupByNameAsync(string name)
        {
            var communityGroupResult =  await _dbContext.CommunityGroup
                              .Where(s => s.IsDeleted == 0 && s.Name == name)
                              .FirstOrDefaultAsync();

            return communityGroupResult;
        }

        public async Task<CommunityGroup> GetCommunityGroupByGuidAsync(Guid communityGroupGuid)
        {

   
            var communityGroupResult = await _dbContext.CommunityGroup
                              .Where(s => s.IsDeleted == 0 && s.CommunityGroupGuid == communityGroupGuid)
                              .FirstOrDefaultAsync();

            return communityGroupResult;
        }

        public async Task<CommunityGroup> GetCommunityGroupByIdAsync(int communityGroupId)
        {
            return await _dbContext.CommunityGroup
              .Where(s => s.IsDeleted == 0 && s.CommunityGroupId == communityGroupId)
              .FirstOrDefaultAsync(); 
        }


       

    }
}
