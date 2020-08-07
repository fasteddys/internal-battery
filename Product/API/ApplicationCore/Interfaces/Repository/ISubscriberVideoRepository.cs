using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberVideoRepository : IUpDiddyRepositoryBase<SubscriberVideo>
    {
        Task<List<SubscriberVideo>> GetSubscriberVideos(Guid subscriberGuid);

        Task<SubscriberVideo> GetSubscriberVideo(Guid subscriberVideoGuid, Guid subscriberGuid);
    }
}
