using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobApplicationService
    {
        Task<bool> IsSubscriberAppliedToJobPosting(int subscriberId, int jobPostingId);
        Task<bool> CreateJobApplication(Guid subscriberGuid, Guid jobGuid, ApplicationDto jobApplicationDto);
        Task<bool> HasJobApplication(Guid subscriberGuid, Guid jobPostingGuid);
    }
}
