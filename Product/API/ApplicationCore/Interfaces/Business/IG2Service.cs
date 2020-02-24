using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IG2Service
    {
        Task<G2SearchResultDto> SearchG2Async(int cityId, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0);
        Task<bool> CreateG2Async(G2Dto g2);

    }
}
