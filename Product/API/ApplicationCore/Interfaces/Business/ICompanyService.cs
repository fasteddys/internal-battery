using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICompanyService
    {
        Task<CompanyDto> GetById(int id);
        Task<List<CompanyDto>> GetCompaniesAsync();
        Task<CompanyDto> GetByCompanyName(string companyName);
        Task AddCompanyAsync(CompanyDto companyDto);
        Task EditCompanyAsync(CompanyDto companyDto);
        Task DeleteCompanyAsync(Guid companyGuid);
    }
}
