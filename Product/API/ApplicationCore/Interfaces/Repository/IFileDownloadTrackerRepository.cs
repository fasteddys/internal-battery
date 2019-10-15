using System;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IFileDownloadTrackerRepository : IUpDiddyRepositoryBase<FileDownloadTracker>
    {
        Task<FileDownloadTracker> GetFileDownloadTrackerByGuidAync(Guid fileDownloadTrackerGuid);
       
    }
}
