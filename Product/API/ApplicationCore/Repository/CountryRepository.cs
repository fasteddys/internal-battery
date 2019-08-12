using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CountryRepository : UpDiddyRepositoryBase<Country>, ICountryRepository
    {
        private readonly IStateRepository _stateRepository;
        private readonly UpDiddyDbContext _dbContext;
        public CountryRepository(UpDiddyDbContext dbContext, IStateRepository stateRepository) : base(dbContext)
        {
            _stateRepository = stateRepository;
            _dbContext = dbContext;
        }

 
        public async Task<IEnumerable<Country>> GetAllCountriesAsync()
        {
            var countries = GetAll();
            return await countries.Join(_stateRepository.GetAllStatesAsync().Result, c=>c.CountryId,s=>s.CountryId,(c,s)=>c)
                            .Distinct()
                            .Where(c => c.IsDeleted == 0)
                            .OrderBy(c => c.Sequence)
                            .ToListAsync();
        }
    }
}
