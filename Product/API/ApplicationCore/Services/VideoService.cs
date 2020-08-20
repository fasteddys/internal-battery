using AutoMapper;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class VideoService : IVideoService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public VideoService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<SubscriberVideoLinksDto> GetSubscriberVideoLink(Guid subscriberGuid)
        {
            var videoLink = await _repositoryWrapper.SubscriberVideoRepository
                .GetExistingSubscriberVideo(subscriberGuid);

            return _mapper.Map<SubscriberVideoLinksDto>(videoLink);
        }

        public async Task SetSubscriberVideoLink(Guid subscriberGuid, SubscriberVideoLinksDto subscriberVideo)
        {
            var videoLink = await _repositoryWrapper.SubscriberVideoRepository
                .GetExistingOrCreateNewSubscriberVideo(subscriberGuid);

            if (videoLink == null) { return; }

            videoLink.ModifyDate = DateTime.UtcNow;
            videoLink.VideoLink = subscriberVideo.VideoLink;
            videoLink.VideoMimeType = subscriberVideo.VideoMimeType;
            videoLink.ThumbnailLink = subscriberVideo.ThumbnailLink;
            videoLink.ThumbnailMimeType = subscriberVideo.ThumbnailMimeType;

            await _repositoryWrapper.SubscriberVideoRepository.SaveAsync();
        }

        public async Task DeleteSubscriberVideoLink(Guid subscriberGuid)
        {
            var videoLink = await _repositoryWrapper.SubscriberVideoRepository
                .GetExistingSubscriberVideo(subscriberGuid);

            if (videoLink == null) { return; }

            _repositoryWrapper.SubscriberVideoRepository.LogicalDelete(videoLink);
            await _repositoryWrapper.SubscriberVideoRepository.SaveAsync();
        }

        public async Task<bool> GetVideoIsVisibleToHiringManager(Guid subscriberGuid)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository
                .GetByGuid(subscriberGuid);

            if (subscriber == null) { throw new NotFoundException($"No subscriber found for \"{subscriberGuid}\""); }
            return subscriber.IsVideoVisibleToHiringManager ?? true;
        }

        public async Task SetVideoIsVisibleToHiringManager(Guid subscriberGuid, bool visibility)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository
                .GetByGuid(subscriberGuid);

            if (subscriber == null) { throw new NotFoundException($"No subscriber found for \"{subscriberGuid}\""); }

            subscriber.IsVideoVisibleToHiringManager = visibility;
            subscriber.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SubscriberRepository.SaveAsync();
        }
    }
}
