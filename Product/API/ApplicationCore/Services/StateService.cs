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
    public class StateService : IStateService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public StateService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<StateDetailDto> GetStateDetail(Guid stateGuid)
        {
            if (stateGuid == null || stateGuid == Guid.Empty)
                throw new NullReferenceException("stateGuid cannot be null");
            IList<StateDetailDto> rval;
            var states = await _repositoryWrapper.State.GetAllStatesAsync();
            if (states == null || states.Count() == 0)
                throw new NotFoundException("States not found");
            rval = _mapper.Map<List<StateDetailDto>>(states);
            return rval?.Where(x => x.StateGuid == stateGuid).FirstOrDefault();
        }

        public async Task<StateDetailListDto> GetStates(Guid countryGuid, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            if (countryGuid == null || countryGuid == Guid.Empty)
                throw new NullReferenceException("CountryGuid cannot be null");
            var states = await _repositoryWrapper.StoredProcedureRepository.GetStates(countryGuid, limit, offset, sort, order);
            if (states == null || states.Count() == 0)
                throw new NotFoundException("States not found");
            return _mapper.Map<StateDetailListDto>(states);
        }

        public async Task CreateState(Guid countryGuid, StateDetailDto stateDetailDto)
        {
            if (countryGuid == null || countryGuid == Guid.Empty || stateDetailDto == null)
                throw new NullReferenceException("StateDetailDto and CountryGuid cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            if (country == null)
                throw new NotFoundException("Country not found");
            var state = _mapper.Map<State>(stateDetailDto);
            state.CreateDate = DateTime.UtcNow;
            state.StateGuid = Guid.NewGuid();
            state.CountryId = country.CountryId;
            await _repositoryWrapper.State.Create(state);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateState(Guid countryGuid, Guid stateGuid, StateDetailDto stateDetailDto)
        {
            if (stateGuid == null || stateGuid == Guid.Empty || countryGuid == null || countryGuid == Guid.Empty)
                throw new NullReferenceException("StateGuid and CountryGuid cannot be null");
            if (stateDetailDto == null)
                throw new NullReferenceException("StateDetailDto cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            if (country == null)
                throw new NotFoundException("Country not found");
            var state = await _repositoryWrapper.State.GetByCountryGuidAndStateGuid(countryGuid, stateGuid);
            if (state == null)
                throw new NotFoundException("State not found");
            state.Name = stateDetailDto.Name;
            state.Sequence = stateDetailDto.Sequence;
            state.Code = stateDetailDto.Code;
            state.CountryId = country.CountryId;
            state.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.State.Update(state);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteState(Guid countryGuid, Guid stateGuid)
        {
            if (stateGuid == null || stateGuid == Guid.Empty || countryGuid == null || countryGuid == Guid.Empty)
                throw new NullReferenceException("StateGuid and CountryGuid cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            if (country == null)
                throw new NotFoundException("Country not found");
            var state = await _repositoryWrapper.State.GetByCountryGuidAndStateGuid(countryGuid, stateGuid);
            if (state == null)
                throw new NotFoundException("State not found");
            state.IsDeleted = 1;
            _repositoryWrapper.State.Update(state);
            await _repositoryWrapper.SaveAsync();
        }
    }
}
