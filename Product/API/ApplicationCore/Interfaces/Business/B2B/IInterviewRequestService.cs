using System;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.B2B
{
    public interface IInterviewRequestService
    {
        Task SubmitInterviewRequest(Guid hiringManagerGuid, Guid profileGuid);
    }
}
