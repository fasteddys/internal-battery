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
        private UpDiddyDbContext _db;
 
        public SubscriberGroupRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;
        }

        public async Task<IList<Subscriber>> GetSubscribersAssociatedWithGroupAsync(int GroupId){
            IList<SubscriberGroup> SubscriberGroups = await _db.SubscriberGroup.Where(sg => sg.GroupId == GroupId && sg.IsDeleted == 0)
                .Include(sg => sg.Subscriber).ToListAsync();
            
            List<Subscriber> Subscribers = new List<Subscriber>();

            foreach(SubscriberGroup sg in SubscriberGroups){
                Subscribers.Add(sg.Subscriber);
            }

            return Subscribers;
        }
    
    }
}
