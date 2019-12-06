using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICountryService
    {
        Task<CountryDetailDto> GetCountryDetail(Guid countryGuid);
        Task<List<CountryDetailDto>> GetAllCountries(int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateCountry(Guid countryGuid, CountryDetailDto countryDetailDto);
        Task CreateCountry(CountryDetailDto countryDetailDto);
        Task DeleteCountry(Guid countryGuid);
    }
}
