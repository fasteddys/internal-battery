using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IG2Service
    {
        Task<G2SearchResultDto> SearchG2Async(Guid subscriberGuid, int cityId,int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0);
        Task<bool> CreateG2Async(G2SDOC g2);

        Task<bool> ReindexSubscriber(Guid subscriber);

    }
}
