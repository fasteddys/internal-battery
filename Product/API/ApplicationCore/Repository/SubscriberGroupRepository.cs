using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class SubscriberGroupRepository : UpDiddyRepositoryBase<SubscriberGroup>, ISubscriberGroupRepository
    {

        private readonly UpDiddyDbContext _dbContext;
        public SubscriberGroupRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext=dbContext;
        }

        public async Task<SubscriberGroup> GetSubscriberGroupByGroupIdSubscriberId(int GroupId, int SubscriberId)
        {
            var subscriberGroup = await GetByConditionAsync(sg=>sg.GroupId==GroupId && sg.SubscriberId==SubscriberId && sg.IsDeleted==0);

            return subscriberGroup.FirstOrDefault();
        }
        
        public async Task CreateSubscriberGroup(SubscriberGroup subscriberGroup)
        {
            await Create(subscriberGroup);
            await SaveAsync();
        }
    }
}
