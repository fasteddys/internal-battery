using System;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITrackingService
    {
        Task RecordSubscriberApplyActionAsync(Guid jobGuid, Guid subscriberGuid);
        Task<Dictionary<Subscriber, List<JobPosting>>> GetSubscriberAbandonedJobPostingHistoryByDateAsync(DateTime datetime);
    }
}
