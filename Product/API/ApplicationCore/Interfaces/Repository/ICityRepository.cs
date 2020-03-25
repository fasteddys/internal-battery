using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICityRepository : IUpDiddyRepositoryBase<City>
    {
        Task<List<CityDetailDto>> GetCities(Guid state, int limit, int offset, string sort, string order);
        Task<IEnumerable<City>> GetCitiesByStateGuid(Guid state);
        Task<City> GetByCityGuid(Guid city);
    }
}