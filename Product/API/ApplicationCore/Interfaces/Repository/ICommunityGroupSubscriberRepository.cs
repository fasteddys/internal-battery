using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICommunityGroupSubscriberRepository : IUpDiddyRepositoryBase<CommunityGroupSubscriber>
    {

        IQueryable<Subscriber> GetAllCommunityGroupSubscribers(Guid communityGroupGuid);
        IQueryable<Subscriber> GetAllCommunityGroupSubscribers(int communityGroupId);

        Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberByLastName(Guid communityGroupGuid, string name);
        Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberByLastName(int communityGroupId, string name);
        Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberById(int CommunityGroupSubscriberId);
        Task<CommunityGroupSubscriber> GetCommunityGroupSubscriberByGuid(Guid CommunityGroupSubscriber);

    }
}
