using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberFileRepository : IUpDiddyRepositoryBase<SubscriberFile>
    {
        IQueryable<SubscriberFile> GetAllSubscriberFileQueryableAsync();
        Task UpdateSubscriberFileAsync(SubscriberFile subscriberFile);
        Task<List<SubscriberFile>> GetAllSubscriberFilesBySubscriberGuid(Guid subscriberFile);
        Task<SubscriberFile> GetMostRecentBySubscriberGuid(Guid subscriberGuid);
        Task<SubscriberFile> GetMostRecentBySubscriberGuidForRecruiter(Guid profileGuid, Guid subscriberGuid);
        Task<DateTime?> GetMostRecentCreatedDate(Guid subscriberGuid);
    }
}
