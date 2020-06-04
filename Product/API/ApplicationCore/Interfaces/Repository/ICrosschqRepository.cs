using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.Models.G2;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICrosschqRepository
    {
        Task<ReferenceCheck> GetReferenceCheckByRequestId(string requestId);
        Task UpdateReferenceCheck(CrosschqWebhookDto crosschqWebhookDto, string fullReportPdfBase64);
    }
}
