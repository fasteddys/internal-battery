using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICountryRepository : IUpDiddyRepositoryBase<Country>
    {
        Task<IEnumerable<Country>> GetAllCountriesAsync();
    }
}
