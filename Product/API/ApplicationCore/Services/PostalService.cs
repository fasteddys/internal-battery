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

namespace UpDiddyApi.ApplicationCore.Services
{
    public class PostalService : IPostalService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public PostalService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
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
    }
}
