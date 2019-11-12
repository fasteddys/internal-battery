using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.IO;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        private readonly IHangfireService _hangfireService;
        private readonly ICloudStorage _cloudStorage;
        private readonly ISubscriberService _subscriberService;

        public ResumeService(IHangfireService hangfireService, IRepositoryWrapper repositoryWrapper, ICloudStorage cloudStorage, ISubscriberService subscriberService)
        {
            _repositoryWrapper = repositoryWrapper;
            _hangfireService = hangfireService;
            _cloudStorage = cloudStorage;
            _subscriberService = subscriberService;
        }

        public async Task UploadResume(Guid subscriberGuid, FileDto fileDto)
        {
            var subscriber = await _subscriberService.GetBySubscriberGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber is not found");
            if(fileDto == null)
                throw new NullReferenceException("FileDto cannot be null");
            var bytes = Convert.FromBase64String(fileDto.Base64EncodedData);
            var resumeStream = new StreamContent(new MemoryStream(bytes));
            var subscriberFiles = await _repositoryWrapper.SubscriberFileRepository.GetAllSubscriberFilesBySubscriberGuid(subscriberGuid);
            string blobName = await _cloudStorage.UploadFileAsync(String.Format("{0}/{1}/", subscriberGuid, "resume"), fileDto.FileName, await resumeStream.ReadAsStreamAsync());
            SubscriberFile subscriberFileResume = new SubscriberFile
            {
                BlobName = blobName,
                ModifyGuid = subscriberGuid,
                CreateGuid = subscriberGuid,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                SubscriberId = subscriber.SubscriberId,
                MimeType = fileDto.MimeType
            };

            if (subscriberFiles.Count > 0)
            {
                SubscriberFile oldFile = subscriberFiles.Last();
                await _cloudStorage.DeleteFileAsync(oldFile.BlobName);
                oldFile.IsDeleted = 1;
            }

            subscriberFiles.Add(subscriberFileResume);
            subscriber.SubscriberFile = subscriberFiles;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task<UpDiddyLib.Domain.Models.FileDto> DownloadResume(Guid subscriberGuid)
        {
            SubscriberFile file = await _repositoryWrapper.SubscriberFileRepository.GetMostRecentBySubscriberGuid(subscriberGuid);
            if (file == null)
                throw new NotFoundException("Resume not found");
            UpDiddyLib.Domain.Models.FileDto resume = new UpDiddyLib.Domain.Models.FileDto();
            resume.MimeType = file.MimeType;
            resume.FileName = file.SimpleName;
            resume.ByteArrayData = Utils.StreamToByteArray(await _cloudStorage.OpenReadAsync(file.BlobName));
            return resume;
        }
    }
}
