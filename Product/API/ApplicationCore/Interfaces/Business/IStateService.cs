using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IStateService
    {        
        Task<StateDetailDto> GetStateDetail(Guid stateGuid);
        Task<StateDetailListDto> GetStates(Guid countryGuid, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateState(StateDetailDto stateDetailDto);
        Task<Guid> CreateState(StateDetailDto stateDetailDto);
        Task DeleteState(Guid stateGuid);
    }
}