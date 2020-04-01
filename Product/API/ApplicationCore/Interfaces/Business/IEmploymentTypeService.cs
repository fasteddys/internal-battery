using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IEmploymentTypeService
    {
        Task<EmploymentTypeDto> GetEmploymentType(Guid employmentTypeGuid);
        Task<EmploymentTypeListDto> GetEmploymentTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateEmploymentType(Guid employmentTypeGuid, EmploymentTypeDto employmentTypeDto);
        Task<Guid> CreateEmploymentType(EmploymentTypeDto employmentTypeDto);
        Task DeleteEmploymentType(Guid employmentTypeGuid);
    }
}
