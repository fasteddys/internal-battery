using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICompanyService
    {
        Task<List<CompanyDto>> GetCompaniesAsync();
        Task AddCompanyAsync(CompanyDto companyDto);
        Task EditCompanyAsync(CompanyDto companyDto);
        Task DeleteCompanyAsync(Guid companyGuid);
    }
}
