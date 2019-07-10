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
        public CompanyRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }

        public async Task AddCompany(Company company)
        {
            await Create(company);
            await SaveAsync();
        }

        public async Task<IQueryable<Company>> GetAllCompanies()
        {
            return await GetAllAsync();
        }

        public async Task<Company> GetCompanyByCompanyGuid(Guid companyGuid)
        {
            var querableCompanies = await GetAllAsync();
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
    }
}
