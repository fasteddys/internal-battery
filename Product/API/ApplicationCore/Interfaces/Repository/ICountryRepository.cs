using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICountryRepository : IUpDiddyRepositoryBase<Country>
    {
        Task<IEnumerable<Country>> GetAllCountriesAsync();
        Task<Country> GetbyCountryGuid(Guid countryGuid);
        Task<List<Country>> GetAllCountries();
    }
}
