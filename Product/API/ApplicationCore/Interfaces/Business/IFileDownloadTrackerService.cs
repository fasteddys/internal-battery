using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IFileDownloadTrackerService
    {
        Task<string> CreateFileDownloadUrl(FileDownloadTrackerDto fileDownloadTrackerDto);
        Task<FileDownloadTrackerDto> GetByFileDownloadTrackerGuid(Guid fileDownloadTrackerGuid);
    }
}
