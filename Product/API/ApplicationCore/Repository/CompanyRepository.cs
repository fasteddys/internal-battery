using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CompanyRepository : UpDiddyRepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }
        public Task<IQueryable<Company>> GetAllCompanies()
        {
            return GetAllAsync();
        }
    }
}
