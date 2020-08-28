using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IVideoService
    {
        Task<SubscriberVideoLinksDto> GetSubscriberVideoLink(Guid subscriberGuid);

        Task SetSubscriberVideoLink(Guid subscriberVideoGuid, SubscriberVideoLinksDto subscriberVideo);

        Task DeleteSubscriberVideoLink(Guid subscriberVideoGuid, Guid subscriberGuid);

        Task Publish(Guid subscriberVideoGuid, Guid subscriberGuid, bool isPublished);

        Task SetVideoIsVisibleToHiringManager(Guid subscriberGuid, bool visibility);
    }
}
