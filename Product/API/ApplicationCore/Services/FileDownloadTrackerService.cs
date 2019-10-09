using System;
using AutoMapper;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Configuration;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class FileDownloadTrackerService : IFileDownloadTrackerService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;


        public FileDownloadTrackerService(IRepositoryWrapper repositoryWrapper, IConfiguration config, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _config = config;
            _mapper = mapper;
        }

        public async Task<string> CreateFileDownloadLink(FileDownloadTrackerDto fileDownloadTrackerDto)
        {
            FileDownloadTracker fileDownloadTracker = new FileDownloadTracker
            {
                SubscriberGuid = fileDownloadTrackerDto.SubscriberGuid,
                FileDownloadTrackerGuid = Guid.NewGuid(),
                MaxFileDownloadAttemptsPermitted = fileDownloadTrackerDto.MaxFileDownloadAttemptsPermitted,
                FileDownloadAttemptCount = 0,
                SourceFileCDNUrl = fileDownloadTrackerDto.SourceFileCDNUrl,
                CreateDate = DateTime.UtcNow

            };
            await _repositoryWrapper.FileDownloadTrackerRepository.Create(fileDownloadTracker);
            await _repositoryWrapper.FileDownloadTrackerRepository.SaveAsync();
            return _config["Environment:BaseUrl"] + "GetFile?f=" + fileDownloadTracker.FileDownloadTrackerGuid;
        }


        public async Task<FileDownloadTrackerDto> GetByFileDownloadTrackerGuid(Guid fileDownloadTrackerGuid)
        {
            var result = await _repositoryWrapper.FileDownloadTrackerRepository.GetFileDownloadTrackerByGuidAync(fileDownloadTrackerGuid);
            if (result != null)
            {
                return _mapper.Map<FileDownloadTracker, FileDownloadTrackerDto>(result);
            }
            else
            {
                return null;
            }
        }

        public async Task Update(FileDownloadTrackerDto fileDownloadTrackerDto)
        {
            FileDownloadTracker tracker = _mapper.Map<FileDownloadTrackerDto, FileDownloadTracker>(fileDownloadTrackerDto);
            tracker.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.FileDownloadTrackerRepository.Update(tracker);
            await _repositoryWrapper.FileDownloadTrackerRepository.SaveAsync();
        }
    }
}