using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IPostalService
    {
        Task<PostalDetailDto> GetPostalDetail(Guid postalGuid);
        Task<PostalDetailListDto> GetPostals(Guid cityGuid, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdatePostal(PostalDetailDto postalDetailDto);
        Task<Guid> CreatePostal(PostalDetailDto postalDetailDto);
        Task DeletePostal(Guid postalGuid);
    }
}