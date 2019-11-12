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
namespace UpDiddyApi.ApplicationCore.Services
{
    public class StateService : IStateService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IMapper _mapper;
        public StateService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _memoryCacheService = memoryCacheService;
            _mapper = mapper;
        }

        public async Task<StateDetailDto> GetStateDetail(Guid stateGuid)
        {
            string cacheKey = $"GetStateDetail";
            IList<StateDetailDto> rval = (IList<StateDetailDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (rval == null)
            {
                var states = await _repositoryWrapper.State.GetAllStatesAsync();
                rval = _mapper.Map<List<StateDetailDto>>(states);
                _memoryCacheService.SetCacheValue(cacheKey, rval);
            }
            return rval?.Where(x => x.StateGuid == stateGuid).FirstOrDefault();
        }

        public async Task<List<StateDetailDto>> GetAllStates(Guid countryGuid, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            var states = await _repositoryWrapper.State.GetByConditionWithSorting(x => x.CountryId == country.CountryId && x.IsDeleted == 0, limit, offset, sort, order);
            if (states == null)
                throw new NotSupportedException("Invalid parameters");
            return _mapper.Map<List<StateDetailDto>>(states);
        }

        public async Task CreateState(Guid countryGuid, StateDetailDto stateDetailDto)
        {
            if (stateDetailDto == null)
                throw new NullReferenceException("stateDetailDto cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            var state = _mapper.Map<State>(stateDetailDto);
            state.CreateDate = DateTime.UtcNow;
            state.StateGuid = Guid.NewGuid();
            state.CountryId = country.CountryId;
            await _repositoryWrapper.State.Create(state);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateState(Guid countryGuid, Guid stateGuid, StateDetailDto stateDetailDto)
        {
            if (stateDetailDto == null)
                throw new NullReferenceException("stateDetailDto cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            var state = await _repositoryWrapper.State.GetByStateGuid(stateGuid);
            state.Name = stateDetailDto.Name;
            state.Sequence = stateDetailDto.Sequence;
            state.Code = stateDetailDto.Code;
            state.CountryId = country.CountryId;
            state.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.State.Update(state);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteState(Guid stateGuid)
        {
            if (stateGuid == null || stateGuid == Guid.Empty)
                throw new NullReferenceException("stateGuid cannot be null");
            var state = await _repositoryWrapper.State.GetByStateGuid(stateGuid);
            state.IsDeleted = 1;
            _repositoryWrapper.State.Update(state);
            await _repositoryWrapper.SaveAsync();
        }
    }
}
