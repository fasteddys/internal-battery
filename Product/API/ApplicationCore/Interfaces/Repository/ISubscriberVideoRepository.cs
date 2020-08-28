using System;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberVideoRepository : IUpDiddyRepositoryBase<SubscriberVideo>
    {
        Task<SubscriberVideo> GetSubscriberVideo(Guid subscriberVideoGuid, Guid subscriberGuid);

        Task<SubscriberVideo> GetExistingOrCreateNewSubscriberVideo(Guid subscriberGuid);
    }
}
