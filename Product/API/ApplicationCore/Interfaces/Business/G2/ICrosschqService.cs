using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Domain.Models.G2.CrossChq;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.G2
{
    public interface ICrosschqService
    {
        Task UpdateReferenceChkStatus(CrosschqWebhookDto request);

        Task<string> CreateReferenceRequest(
            ProfileDto profile,
            RecruiterInfoDto recruiter,
            CrossChqReferenceRequestDto referenceRequest);
    }
}
