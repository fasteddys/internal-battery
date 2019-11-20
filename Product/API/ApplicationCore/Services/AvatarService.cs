
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class AvatarService : IAvatarService
    {
        private IConfiguration _configuration { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;

        public AvatarService(IConfiguration configuration, IRepositoryWrapper repository, IMapper mapper)
        {
            _configuration = configuration;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<string> GetAvatar(Guid subscriberGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            Subscriber subscriber = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            if (string.IsNullOrEmpty(subscriber.AvatarUrl))
                throw new NotFoundException("Subscriber avatar does not exist");
            return _configuration["StorageAccount:AssetBaseUrl"] + subscriber.AvatarUrl + "?" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        }

        public async Task UploadAvatar(Guid subscriberGuid, FileDto fileDto)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            if (fileDto == null)
                throw new NullReferenceException("FileDto cannot be null");
            if (string.IsNullOrEmpty(fileDto.MimeType))
                throw new NullReferenceException("FileDto.MimeType cannot be null or empty");
            if (string.IsNullOrEmpty(fileDto.FileName))
                throw new NullReferenceException("FileDto.FileName cannot be null or empty");
            if (string.IsNullOrEmpty(fileDto.Base64EncodedData))
                throw new NullReferenceException("FileDto.Base64EncodedData cannot be null or empty");
            Subscriber subscriber = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            var bytes = Convert.FromBase64String(fileDto.Base64EncodedData);
            int MaxAvatarFileSize = int.Parse(_configuration["CareerCircle:MaxAvatarFileSize"]);
            if (bytes.Length > MaxAvatarFileSize)
                throw new FileSizeExceedsLimit($"Avatar file cannot exceeds size limit of {MaxAvatarFileSize}");
            AzureBlobStorage abs = new AzureBlobStorage(_configuration);
            string blobFilePath = subscriberGuid + _configuration["CareerCircle:AvatarName"];
            await abs.UploadBlobAsync(blobFilePath, bytes);
            subscriber.AvatarUrl = blobFilePath;
            subscriber.ModifyDate = DateTime.UtcNow;
            await _repository.SaveAsync();
        }

        public async Task RemoveAvatar(Guid subscriberGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            Subscriber subscriber = _repository.SubscriberRepository.GetSubscriberByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            subscriber.AvatarUrl = string.Empty;
            subscriber.ModifyDate = DateTime.UtcNow;
            await _repository.SaveAsync();
        }
    }
}
