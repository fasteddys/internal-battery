using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.CrossChq;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICrosschqRepository
    {
        Task<ReferenceCheck> GetReferenceCheckByRequestId(string requestId);
        Task<ReferenceCheckReport> GetReferenceCheckReportPdf(Guid referenceCheckGuid, string reportType);
        Task UpdateReferenceCheck(CrosschqWebhookDto crosschqWebhookDto, string fullReportPdfBase64, string summaryReportPdfBase64);
    }
}
