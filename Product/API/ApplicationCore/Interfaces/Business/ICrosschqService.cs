using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Domain.Models.CrossChq;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICrosschqService
    {
        Task UpdateReferenceChkStatus(CrosschqWebhookDto request);

        Task<string> CreateReferenceRequest(
            ProfileDto profile,
            RecruiterInfoDto recruiter,
            CrossChqReferenceRequestDto referenceRequest);

        Task<CrossChqReferenceResponse> RetrieveReferenceStatus(ProfileDto profile);

        Task<ReferenceCheckReportDto> GetReferenceCheckReportPdf(Guid referenceCheckGuid, string reportType);
    }
}
