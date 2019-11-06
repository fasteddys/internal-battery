using System;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobApplicationService
    {
        Task<bool> IsSubscriberAppliedToJobPosting(int subscriberId, int jobPostingId);
        Task<bool> CreateJobApplication(Guid subscriberGuid, Guid jobGuid, ApplicationDto jobApplicationDto);
    }
}
