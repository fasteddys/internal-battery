using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CityService : ICityService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IMemoryCacheService _memoryCacheService;

        public CityService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _memoryCacheService = memoryCacheService;
        }

        public async Task<Guid> CreateCity(CityDetailDto cityDetailDto)
        {
            if (cityDetailDto == null)
                throw new NullReferenceException("cityDetailDto cannot be null");
            var state = await _repositoryWrapper.State.GetByStateGuid(cityDetailDto.StateGuid);
            if (state == null)
                throw new NotFoundException("State not found");
            var city = _mapper.Map<City>(cityDetailDto);
            city.CreateDate = DateTime.UtcNow;
            city.CityGuid = Guid.NewGuid();
            city.StateId = state.StateId;
            await _repositoryWrapper.CityRepository.Create(city);
            await _repositoryWrapper.SaveAsync();
            return city.CityGuid.Value;
        }

        public async Task DeleteCity(Guid cityGuid)
        {
            if (cityGuid == null || cityGuid == Guid.Empty)
                throw new NullReferenceException("cityGuid cannot be null");
            var city = await _repositoryWrapper.CityRepository.GetByCityGuid(cityGuid);
            if (city == null)
                throw new NotFoundException("City not found");
            city.IsDeleted = 1;
            city.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.CityRepository.Update(city);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task<CityDetailDto> GetCityDetail(Guid cityGuid)
        {
            if (cityGuid == null || cityGuid == Guid.Empty)
                throw new NullReferenceException("cityGuid cannot be null");
            var city = await _repositoryWrapper.CityRepository.GetByCityGuid(cityGuid);
            if (city == null)
                throw new NotFoundException($"City with guid: {cityGuid} does not exist");
            return _mapper.Map<CityDetailDto>(city);
        }

        public async Task<CityDetailListDto> GetCities(Guid stateGuid, int limit, int offset, string sort, string order)
        {
            if (stateGuid == null || stateGuid == Guid.Empty)
                throw new NullReferenceException("stateGuid cannot be null");
            var state = await _repositoryWrapper.State.GetByStateGuid(stateGuid);
            if (state == null)
                throw new NotFoundException("State not found");
            var cities = await _repositoryWrapper.CityRepository.GetCities(stateGuid, limit, offset, sort, order);
            if (cities == null || cities.Count() == 0)
                return new CityDetailListDto() { Cities = new List<CityDetailDto>(), TotalRecords = 0 };
            return _mapper.Map<CityDetailListDto>(cities);
        }

        public async Task UpdateCity(CityDetailDto cityDetailDto)
        {
            if (cityDetailDto == null)
                throw new NullReferenceException("cityDetailDto cannot be null");
            var city = await _repositoryWrapper.CityRepository.GetByCityGuid(cityDetailDto.CityGuid);
            if (city == null)
                throw new NotFoundException("City not found");
            city.Name = cityDetailDto.Name;
            city.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.CityRepository.Update(city);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task<List<CityStateSearchDto>> GetCityByKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                throw new NullReferenceException("Value cannot be null or empty");
            keyword = keyword.ToLower();
            string searchCacheKey = $"GetCityStateSearchTerm" + keyword;
            List<CityStateSearchDto> rval = (List<CityStateSearchDto>)_memoryCacheService.GetCacheValue(searchCacheKey);
            if (rval == null)
            {
                string cacheKey = $"GetCityStateSearchTerm";
                List<CityStateSearchDto> searchList = (List<CityStateSearchDto>)_memoryCacheService.GetCacheValue(cacheKey);
                if (searchList == null)
                {
                    searchList = await _repositoryWrapper.CityRepository.SearchByKeyword();
                    _memoryCacheService.SetCacheValue(cacheKey, searchList);
                }
                rval = searchList?.Where(v => v.Name.ToLower().Contains(keyword))?.ToList();
                _memoryCacheService.SetCacheValue(searchCacheKey, rval);
            }
            return rval;
        }
    }
}
