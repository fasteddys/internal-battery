using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class FileDownloadTrackerRepository : UpDiddyRepositoryBase<FileDownloadTracker>, IFileDownloadTrackerRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public FileDownloadTrackerRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async  Task<FileDownloadTracker> GetFileDownloadTrackerByGuidAync(Guid fileDownloadTrackerGuid)
        {
            return await _dbContext.FileDownloadTracker.Where(x => x.FileDownloadTrackerGuid == fileDownloadTrackerGuid).AsNoTracking().FirstOrDefaultAsync();
        }
    }
}
