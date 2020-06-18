using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IStateRepository : IUpDiddyRepositoryBase<State>
    {
        Task AddUSState(State state);
        Task<IEnumerable<State>> GetAllStatesAsync();
        Task<IEnumerable<State>> GetStatesByCountryGuid(Guid countryGuid);
        Task<IEnumerable<State>> GetStatesForDefaultCountry();
        Task<State> GetStateBySubscriberGuid(Guid subscriberGuid);
        Task<State> GetByStateGuid(Guid stateGuid);

        Task<State> GetUSCanadaStateByCode(string stateCode);
    }
}