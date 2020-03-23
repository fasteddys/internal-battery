using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class PostalRepository : UpDiddyRepositoryBase<Postal>, IPostalRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public PostalRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Postal> GetByPostalGuid(Guid postal)
        {
            return await (from p in _dbContext.Postal.Include(p => p.City)
                          where p.PostalGuid == postal
                          select p).FirstOrDefaultAsync();
        }

        public async Task<List<PostalDetailDto>> GetPostals(Guid city, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@City", city),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<PostalDetailDto> rval = null;
            rval = await _dbContext.Postals.FromSql<PostalDetailDto>("System_Get_Postals @City, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<IEnumerable<Postal>> GetPostalsByCityGuid(Guid city)
        {
            var postals = GetAll();
            return await postals.Include(p => p.City)
                .Where(p => p.IsDeleted == 0 && p.City.CityGuid == city)
                .ToListAsync();
        }
    }
}
