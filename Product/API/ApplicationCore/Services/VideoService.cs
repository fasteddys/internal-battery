using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class VideoService : IVideoService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public VideoService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        // TODOs:
        //
        // 1. Create new, empty video entry, return GUID
        // 1.a. what do we do with previous recordings?  When do we "Delete" those?
        // 2. Save video link by video GUID and subscriberGuid
        // 3. Ditto for thumbnail
        // 3.a save thumbnail at the same time as the video or separately?
        // 4.  Retrieve video
        // 5.  Retrieve thumbnail single
        // 5.a  Retrieve videos and thumbnail at the same time?
        // 6.  Retrieve thumbnail List

        public async Task ToggleVideoVisibilityForHiringManager(bool isVisible, Guid subscriberGuid)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository
                .GetByGuid(subscriberGuid);

            if (subscriber == null) { throw new NotFoundException($"No subscriber found for \"{subscriberGuid}\""); }

            subscriber.IsVideoVisibleToHiringManager = isVisible;
            await _repositoryWrapper.SubscriberRepository.SaveAsync();
        }
    }
}
