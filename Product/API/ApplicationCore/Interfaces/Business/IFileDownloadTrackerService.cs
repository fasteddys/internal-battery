using System;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IFileDownloadTrackerService
    {
        Task<string> CreateFileDownloadLink(FileDownloadTrackerDto fileDownloadTrackerDto);
        Task<FileDownloadTrackerDto> GetByFileDownloadTrackerGuid(Guid fileDownloadTrackerGuid);
        Task Update(FileDownloadTrackerDto fileDownloadTrackerDto);
        Task<string> GetFileUrlByFileDownloadTrackerGuid(Guid fileDownloadTrackerGuid);
    }
}
