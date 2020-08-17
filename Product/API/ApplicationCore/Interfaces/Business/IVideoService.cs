using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IVideoService
    {
        Task<SubscriberVideoLinksDto> GetSubscriberVideoLink(Guid subscriberGuid);

        Task SetSubscriberVideoLink(Guid subscriberGuid, SubscriberVideoLinksDto subscriberVideo);

        Task DeleteSubscriberVideoLink(Guid subscriberGuid);

        Task<bool> GetVideoIsVisibleToHiringManager(Guid subscriberGuid);

        Task SetVideoIsVisibleToHiringManager(Guid subscriberGuid, bool visibility);
    }
}
