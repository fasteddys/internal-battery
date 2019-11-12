using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using AutoMapper;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class CountryService : ICountryService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IMapper _mapper;
        public CountryService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _memoryCacheService = memoryCacheService;
            _mapper = mapper;
        }

        public async Task<CountryDetailDto> GetCountryDetail(Guid countryGuid)
        {
            if (countryGuid == null || countryGuid == Guid.Empty)
                throw new NullReferenceException("countryGuid cannot be null");
            string cacheKey = $"GetCountryDetail";
            IList<CountryDetailDto> rval = (IList<CountryDetailDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (rval == null)
            {
                var countries = await _repositoryWrapper.Country.GetAllCountries();
                if (countries == null)
                    throw new NotFoundException("country not found");
                rval = _mapper.Map<List<CountryDetailDto>>(countries);
                _memoryCacheService.SetCacheValue(cacheKey, rval);
            }
            return rval?.Where(x => x.CountryGuid == countryGuid).FirstOrDefault();
        }

        public async Task<List<CountryDetailDto>> GetAllCountries(int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var country = await _repositoryWrapper.Country.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            if (country == null)
                throw new NotFoundException("Country not found");
            return _mapper.Map<List<CountryDetailDto>>(country);
        }

        public async Task CreateCountry(CountryDetailDto countryDetailDto)
        {
            if (countryDetailDto == null)
                throw new NullReferenceException("countryDetailDto cannot be null");
            var country = _mapper.Map<Country>(countryDetailDto);
            country.CreateDate = DateTime.UtcNow;
            country.CountryGuid = Guid.NewGuid();
            await _repositoryWrapper.Country.Create(country);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateCountry(Guid countryGuid, CountryDetailDto countryDetailDto)
        {
            if (countryDetailDto == null || countryGuid == null || countryGuid == Guid.Empty)
                throw new NullReferenceException("countryDetailDto and countryGuid cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            if (country == null)
                throw new NotFoundException("Country not found");
            country.OfficialName = countryDetailDto.OfficialName;
            country.DisplayName = countryDetailDto.DisplayName;
            country.Sequence = countryDetailDto.Sequence;
            country.Code2 = countryDetailDto.Code2;
            country.Code3 = countryDetailDto.Code3;
            country.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.Country.Update(country);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteCountry(Guid countryGuid)
        {
            if (countryGuid == null || countryGuid == Guid.Empty)
                throw new NullReferenceException("CountryGuid cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            if (country == null)
                throw new NotFoundException("Country not found");
            country.IsDeleted = 1;
            _repositoryWrapper.Country.Update(country);
            await _repositoryWrapper.SaveAsync();
        }
    }
}
