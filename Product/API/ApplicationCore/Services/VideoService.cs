using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class VideoService : IVideoService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        private ICloudStorage _cloudStorage { get; set; }


        public VideoService(IRepositoryWrapper repositoryWrapper, ICloudStorage cloudStorage,
        IMapper mapper)
        {

            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _cloudStorage = cloudStorage;
        }

        public async Task<SubscriberVideoLinksDto> GetSubscriberVideoLink(Guid subscriberGuid, bool isPreview)
        {
            var videoLink = await _repositoryWrapper.SubscriberVideoRepository
                .GetExistingOrCreateNewSubscriberVideo(subscriberGuid);
            var dto = _mapper.Map<SubscriberVideoLinksDto>(videoLink);
            if (dto.VideoLink.Length > 0 && dto.ThumbnailLink.Length > 0)
            {
                if(isPreview)
                {
                    dto.VideoLink.Replace("/video.","/video_preview.");
                    dto.ThumbnailLink.Replace("/thumbnail.","/thumbnail_preview.");
                }
              string videoSAS  = await _cloudStorage.GetBlobSAS(dto.VideoLink);
              string thumbnailSAS = await _cloudStorage.GetBlobSAS(dto.ThumbnailLink);
              dto.VideoLink = dto.VideoLink + "?" + videoSAS;
              dto.ThumbnailLink = dto.ThumbnailLink + "?" + thumbnailSAS;
            }
            return dto;
        }

        public async Task SetSubscriberVideoLink(Guid subscriberVideoGuid, SubscriberVideoLinksDto subscriberVideo)
        {
            var videoLink = await _repositoryWrapper.SubscriberVideoRepository
                .GetByGuid(subscriberVideoGuid);

            if (videoLink == null) { throw new NotFoundException(); }

            videoLink.ModifyDate = DateTime.UtcNow;
            videoLink.VideoLink = subscriberVideo.VideoLink;
            videoLink.VideoMimeType = subscriberVideo.VideoMimeType;
            videoLink.ThumbnailLink = subscriberVideo.ThumbnailLink;
            videoLink.ThumbnailMimeType = subscriberVideo.ThumbnailMimeType;

            await _repositoryWrapper.SubscriberVideoRepository.SaveAsync();
        }

        public async Task DeleteSubscriberVideoLink(Guid subscriberVideoGuid, Guid subscriberGuid)
        {
            var videoLink = await _repositoryWrapper.SubscriberVideoRepository
                .GetSubscriberVideo(subscriberVideoGuid, subscriberGuid);

            if (videoLink == null) { throw new NotFoundException(); }

            _repositoryWrapper.SubscriberVideoRepository.LogicalDelete(videoLink);
            await _repositoryWrapper.SubscriberVideoRepository.SaveAsync();
        }

        public async Task Publish(Guid subscriberVideoGuid, Guid subscriberGuid, bool isPublished)
        {
            var videoLink = await _repositoryWrapper.SubscriberVideoRepository
                .GetSubscriberVideo(subscriberVideoGuid, subscriberGuid);

            if (videoLink == null) { throw new NotFoundException(); }

            videoLink.IsPublished = isPublished;
            await _repositoryWrapper.SubscriberVideoRepository.SaveAsync();
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
