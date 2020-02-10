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
        Task<CompanyListDto> GetCompanies(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<CompanyDto> GetByCompanyName(string companyName);
        Task<CompanyDto> GetByCompanyGuid(Guid companyGuid);
        Task<Guid> AddCompanyAsync(CompanyDto companyDto);
        Task EditCompanyAsync(CompanyDto companyDto);
        Task DeleteCompanyAsync(Guid companyGuid);
    }
}
