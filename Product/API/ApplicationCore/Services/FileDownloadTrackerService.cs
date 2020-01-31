using System;
using AutoMapper;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Configuration;
using EntityTypeConst = UpDiddyLib.Helpers.Constants.EventType;
using UpDiddyApi.ApplicationCore.Exceptions;

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
                SubscriberId = fileDownloadTrackerDto.SubscriberId,
                FileDownloadTrackerGuid = Guid.NewGuid(),
                MaxFileDownloadAttemptsPermitted = fileDownloadTrackerDto.MaxFileDownloadAttemptsPermitted,
                FileDownloadAttemptCount = 0,
                GroupId = fileDownloadTrackerDto.GroupId,
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

        public async Task<string> GetFileUrlByFileDownloadTrackerGuid(Guid fileDownloadTrackerGuid)
        {
            FileDto fileDto = new FileDto();
            string fileUrl = null;
            var result = await _repositoryWrapper.FileDownloadTrackerRepository.GetFileDownloadTrackerByGuidAync(fileDownloadTrackerGuid);
            UpDiddyLib.Dto.FileDownloadTrackerDto trackerDto = _mapper.Map<FileDownloadTracker, FileDownloadTrackerDto>(result);
            if (trackerDto == null)
            {
                throw new NotFoundException("FileDownloadTracker not found");
            }
            if ((trackerDto.MaxFileDownloadAttemptsPermitted != null && trackerDto.FileDownloadAttemptCount <= trackerDto.MaxFileDownloadAttemptsPermitted) || trackerDto.MaxFileDownloadAttemptsPermitted == null)
            {
                result.FileDownloadAttemptCount++;
                result.MostrecentfiledownloadAttemptinUtc = DateTime.UtcNow;
                fileUrl = trackerDto.SourceFileCDNUrl;
                _repositoryWrapper.FileDownloadTrackerRepository.Update(result);
                Models.Action action = await _repositoryWrapper.ActionRepository.GetByNameAsync(UpDiddyLib.Helpers.Constants.Action.DownloadGatedFile);
                EntityType entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync(EntityTypeConst.FileDownloadTracker);
                SubscriberAction subAction = new SubscriberAction()
                {
                    IsDeleted = 0,
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = null,
                    Action = action,
                    CreateGuid = Guid.Empty,
                    SubscriberId = result.SubscriberId,
                    SubscriberActionGuid = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityId = result.FileDownloadTrackerId,
                    ModifyGuid = null,
                    OccurredDate = DateTime.UtcNow
                };
                await _repositoryWrapper.SubscriberActionRepository.Create(subAction);
                await _repositoryWrapper.SaveAsync();
            }
            else
            {
                throw new MaximumReachedException("Maximum number of download attempt has been reached.");
            }
            return fileUrl;
        }
    }
}