using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.G2
{
    public interface ICrosschqService
    {
        Task UpdateReferenceChkStatus(CrosschqWebhookDto request);
    }
}
