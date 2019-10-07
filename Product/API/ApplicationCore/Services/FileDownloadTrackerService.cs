using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        public async Task<string> CreateFileDownloadUrl(FileDownloadTrackerDto fileDownloadTrackerDto)
        {
                FileDownloadTracker fileDownloadTracker = new FileDownloadTracker{
                    SubscriberGuid = fileDownloadTrackerDto.SubscriberGuid,
                    FileDownloadTrackerGuid = Guid.NewGuid(),
                    MaxFileDownloadAttemptsPermitted = 5,
                    FileDownloadAttemptCount = 0,
                    SourceFileCDNUrl = fileDownloadTrackerDto.SourceFileCDNUrl
                    
                };
                await _repositoryWrapper.FileDownloadTrackerRepository.Create(fileDownloadTracker);
                await _repositoryWrapper.FileDownloadTrackerRepository.SaveAsync();

                string url = _config["Environment:Base"] + "/filedownload/" + fileDownloadTracker.FileDownloadTrackerGuid;
                return url;
        }
        

        public async Task<FileDownloadTrackerDto> GetByFileDownloadTrackerGuid(Guid fileDownloadTrackerGuid)
        {
            var result = await _repositoryWrapper.FileDownloadTrackerRepository.GetFileDownloadTrackerByGuidAync(fileDownloadTrackerGuid);
            if(result != null)
            {
                return _mapper.Map<FileDownloadTracker,FileDownloadTrackerDto>(result);
            }
            else
            {
                return null;
            }            
        }

        public void DownloadFile(Guid fileGuid)
        {
            //Grab the file from CDN and return to webapp
            //Track the action by user
            //Increment the download count on FileDownloadTracker table
        }

        
 
    }
}