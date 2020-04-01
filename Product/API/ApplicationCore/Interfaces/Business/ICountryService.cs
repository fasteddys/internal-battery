using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICountryService
    {
        Task<CountryDetailDto> GetCountryDetail(Guid countryGuid);
        Task<CountryDetailListDto> GetAllCountries(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateCountry(CountryDetailDto countryDetailDto);
        Task<Guid> CreateCountry(CountryDetailDto countryDetailDto);
        Task DeleteCountry(Guid countryGuid);
    }
}
