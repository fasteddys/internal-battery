using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICompensationTypeService
    {
        Task<CompensationTypeDto> GetCompensationType(Guid compensationTypeGuid);
        Task<CompensationTypeListDto> GetCompensationTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateCompensationType(Guid compensationTypeGuid, CompensationTypeDto compensationTypeDto);
        Task CreateCompensationType(CompensationTypeDto compensationTypeDto);
        Task DeleteCompensationType(Guid compensationTypeGuid);
    }
}
