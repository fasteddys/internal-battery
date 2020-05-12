using System;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.B2B
{
    public interface IInterviewRequestService
    {
        Task<Guid> SubmitInterviewRequest(HiringManagerDto hiringManager, Guid profileGuid, bool nonBlocking = true);
    }
}
