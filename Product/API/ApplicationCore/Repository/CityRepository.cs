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
    }
}
