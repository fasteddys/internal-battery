using Microsoft.EntityFrameworkCore;
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
        private readonly UpDiddyDbContext _dbContext;
        public CompanyRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
                _dbContext = dbContext;
        }

        public async Task AddCompany(Company company)
        {
            await Create(company);
            await SaveAsync();
        }

        public  IQueryable<Company> GetAllCompanies()
        {
            return  GetAll();
        }

        public async Task<Company> GetCompanyByCompanyGuid(Guid companyGuid)
        {
            var querableCompanies =  GetAll();
            var companyResult = await querableCompanies
                            .Where(c=> c.IsDeleted == 0 && c.CompanyGuid == companyGuid)
                            .ToListAsync();

            return companyResult.Count == 0 ? null : companyResult[0];
        }

        public async Task UpdateCompany(Company company)
        {
            Update(company);
            await SaveAsync();
        }

        public async Task<List<Company>> GetAllCompanyEntities()
        {
            return await (from e in _dbContext.Company
                          where e.IsDeleted == 0
                          select e ).ToListAsync();
        }
    }
}
