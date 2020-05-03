using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IPostalRepository : IUpDiddyRepositoryBase<Postal>
    {
        Task<List<PostalDetailDto>> GetPostals(Guid city, int limit, int offset, string sort, string order);
        Task<IEnumerable<Postal>> GetPostalsByCityGuid(Guid city);
        Task<Postal> GetByPostalGuid(Guid PostalGuid);
        Task<List<PostalLookupDto>> GetAllUSPostals();
    }
}
