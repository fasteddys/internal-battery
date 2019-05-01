using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class StateRepository : UpDiddyRepositoryBase<State>, IStateRepository
    {
        public StateRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }

        public async Task<IEnumerable<State>> GetAllStatesAsync()
        {
            var states = GetAllAsync();
            return await states.Result
                               .Include(s => s.Country)
                               .ToListAsync();                             
        }

        public async Task<IEnumerable<State>> GetStatesByCountryGuid(Guid countryGuid)
        {
            var states = GetAllAsync();
            return await states.Result
                                .Include(s => s.Country)
                                .Where(s=>s.IsDeleted==0 && s.Country.CountryGuid== countryGuid)
                                .ToListAsync();
        }

        public async Task<IEnumerable<State>> GetStatesForDefaultCountry()
        {
            var states = GetAllAsync();
            return await states.Result
                               .Include(s => s.Country)
                               .Where(s => s.IsDeleted == 0 && s.Country.Sequence == 1)
                               .OrderBy(s=>s.Sequence)
                               .ToListAsync();
        }
    }
}
