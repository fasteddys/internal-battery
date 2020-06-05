using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.CrossChq;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICrosschqService
    {
        Task UpdateReferenceChkStatus(CrosschqWebhookDto request);

        Task<string> CreateReferenceRequest(
            Guid profileGuid,
            Guid subscriberGuid,
            CrossChqReferenceRequestDto referenceRequest);

        Task<List<ReferenceStatusDto>> RetrieveReferenceStatus(Guid profileGuid, Guid subscriberGuid);

        Task<ReferenceCheckReportDto> GetReferenceCheckReportPdf(Guid referenceCheckGuid, string reportType);
    }
}
