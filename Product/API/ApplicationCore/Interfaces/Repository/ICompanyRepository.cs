using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICompanyRepository : IUpDiddyRepositoryBase<Company>
    {
        IQueryable<Company> GetAllCompanies();
        Task AddCompany(Company company);
        Task<Company> GetCompanyByCompanyGuid(Guid companyGuid);
        Task UpdateCompany(Company company);
    }
}
