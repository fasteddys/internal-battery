using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class PostalService : IPostalService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IMemoryCacheService _memoryCacheService;

        public PostalService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _memoryCacheService = memoryCacheService;
        }

        public async Task<Guid> CreatePostal(PostalDetailDto postalDetailDto)
        {
            if (postalDetailDto == null)
                throw new NullReferenceException("postalDetailDto cannot be null");
            var city = await _repositoryWrapper.CityRepository.GetByCityGuid(postalDetailDto.CityGuid);
            if (city == null)
                throw new NotFoundException("City not found");
            var postal = _mapper.Map<Postal>(postalDetailDto);
            postal.CreateDate = DateTime.UtcNow;
            postal.PostalGuid = Guid.NewGuid();
            postal.CityId = city.CityId;
            await _repositoryWrapper.PostalRepository.Create(postal);
            await _repositoryWrapper.SaveAsync();
            return postal.PostalGuid.Value;
        }

        public async Task DeletePostal(Guid postalGuid)
        {
            if (postalGuid == null || postalGuid == Guid.Empty)
                throw new NullReferenceException("postalGuid cannot be null");
            var postal = await _repositoryWrapper.PostalRepository.GetByPostalGuid(postalGuid);
            if (postal == null)
                throw new NotFoundException("Postal not found");
            postal.IsDeleted = 1;
            postal.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.PostalRepository.Update(postal);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task<PostalDetailDto> GetPostalDetail(Guid postalGuid)
        {
            if (postalGuid == null || postalGuid == Guid.Empty)
                throw new NullReferenceException("postalGuid cannot be null");
            var postal = await _repositoryWrapper.PostalRepository.GetByPostalGuid(postalGuid);
            if (postal == null)
                throw new NotFoundException($"Postal with guid: {postalGuid} does not exist");
            return _mapper.Map<PostalDetailDto>(postal);
        }

        public async Task<PostalDetailListDto> GetPostals(Guid cityGuid, int limit, int offset, string sort, string order)
        {
            if (cityGuid == null || cityGuid == Guid.Empty)
                throw new NullReferenceException("cityGuid cannot be null");
            var city = await _repositoryWrapper.CityRepository.GetByCityGuid(cityGuid);
            if (city == null)
                throw new NotFoundException("City not found");
            var postals = await _repositoryWrapper.PostalRepository.GetPostals(cityGuid, limit, offset, sort, order);
            if (postals == null || postals.Count() == 0)
                return new PostalDetailListDto() { Postals = new List<PostalDetailDto>(), TotalRecords = 0 };
            return _mapper.Map<PostalDetailListDto>(postals);
        }

        public async Task UpdatePostal(PostalDetailDto postalDetailDto)
        {
            if (postalDetailDto == null)
                throw new NullReferenceException("postalDetailDto cannot be null");
            var postal = await _repositoryWrapper.PostalRepository.GetByPostalGuid(postalDetailDto.PostalGuid);
            if (postal == null)
                throw new NotFoundException("Postal not found");
            postal.Code = postalDetailDto.Code;
            postal.Latitude = postalDetailDto.Latitude;
            postal.Longitude = postalDetailDto.Longitude;
            postal.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.PostalRepository.Update(postal);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task<List<PostalLookupDto>> GetPostalsLookup(string value)
        {
            if (value == null || value.Length != 5 || !value.All(c => c >= '0' && c <= '9'))
                throw new FailedValidationException("lookup value must be five characters in length and may only contain digits");
            string cacheKey = "AllUSPostals";
            List<PostalLookupDto> cachedUSPostals = (List<PostalLookupDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (cachedUSPostals == null)
            {
                cachedUSPostals = await _repositoryWrapper.PostalRepository.GetAllUSPostals();
                _memoryCacheService.SetCacheValue(cacheKey, cachedUSPostals, 60 * 24);
            }
            return cachedUSPostals.Where(x => x.PostalCode == value).ToList();
        }
    }
}