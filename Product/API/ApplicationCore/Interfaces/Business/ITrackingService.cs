using System;
using System.Collections.Generic;
using UpDiddyApi.Models;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITrackingService
    {
        Task RecordSubscriberApplyActionAsync(Guid jobGuid, Guid subscriberGuid);
        Task<Dictionary<Subscriber, List<JobPosting>>> GetSubscriberAbandonedJobPostingHistoryByDateAsync(DateTime datetime);
        Task TrackingSubscriberJobViewAction(Guid jobGuid, Guid subscriberGuid);
        Task TrackingSubscriberFileDownloadAction(Guid subscriberGuid, int fileDownloadTrackerId);
    }
}
