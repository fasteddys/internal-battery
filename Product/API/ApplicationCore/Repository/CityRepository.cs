using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CityRepository : UpDiddyRepositoryBase<City>, ICityRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public CityRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<City> GetByCityGuid(Guid city)
        {
            return await (from c in _dbContext.City.Include(c => c.State)
                          where c.CityGuid == city
                          && c.IsDeleted == 0
                          select c).FirstOrDefaultAsync();
        }

        public async Task<List<CityDetailDto>> GetCities(Guid state, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@State", state),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CityDetailDto> rval = null;
            rval = await _dbContext.Cities.FromSql<CityDetailDto>("System_Get_Cities @State, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<IEnumerable<City>> GetCitiesByStateGuid(Guid state)
        {
            var cities = GetAll();
            return await cities.Include(c => c.State)
                .Where(c => c.IsDeleted == 0 && c.State.StateGuid == state)
                .ToListAsync();
        }

        public async Task<List<CityStateSearchDto>> SearchByKeyword()
        {
            return await (from c in _dbContext.City
                          join s in _dbContext.State on c.StateId equals s.StateId
                          join co in _dbContext.Country on s.CountryId equals co.CountryId
                          where co.CountryGuid == new Guid("8b5dec9a-b5cf-4bdc-b015-ccfd4339d32b")
                          && s.IsDeleted == 0 && co.IsDeleted == 0
                          orderby c.Name
                          select new CityStateSearchDto()
                          {
                              Name = c.Name + ", " + s.Name,
                              CityGuid = c.CityGuid.Value
                          }).ToListAsync();

        }

        public async Task<List<CityStateLookupDto>> GetAllUSCitiesAndStates()
        {
            return await (from co in _dbContext.Country
                          join s in _dbContext.State on co.CountryId equals s.CountryId
                          join ci in _dbContext.City on s.StateId equals ci.StateId
                          where co.Code3 == "USA" && s.IsDeleted == 0 && co.IsDeleted == 0 && ci.IsDeleted == 0
                          select new CityStateLookupDto()
                          {
                              CityGuid = ci.CityGuid.Value,
                              CityName = ci.Name,
                              StateGuid = s.StateGuid.Value,
                              StateName = s.Name,
                              StateCode = s.Code
                          }).ToListAsync();
        }
    }
}
