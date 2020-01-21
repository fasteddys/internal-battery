using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IIndustryService
    {
        Task<IndustryDto> GetIndustry(Guid industryGuid);
        Task<IndustryListDto> GetIndustries(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateIndustry(Guid industryGuid, IndustryDto industryDto);
        Task CreateIndustry(IndustryDto industryDto);
        Task DeleteIndustry(Guid industryGuid);
    }
}
