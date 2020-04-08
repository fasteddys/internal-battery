using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICityService
    {
        Task<CityDetailDto> GetCityDetail(Guid cityGuid);
        Task<CityDetailListDto> GetCities(Guid stateGuid, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateCity(CityDetailDto cityDetailDto);
        Task<Guid> CreateCity(CityDetailDto cityDetailDto);
        Task DeleteCity(Guid cityGuid);
    }
}