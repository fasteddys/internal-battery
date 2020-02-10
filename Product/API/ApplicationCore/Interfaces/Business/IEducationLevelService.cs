using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IEducationLevelService
    {
        Task<EducationLevelDto> GetEducationLevel(Guid educationLevelGuid);
        Task<EducationLevelListDto> GetEducationLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateEducationLevel(Guid EducationLevelGuid, EducationLevelDto educationLevelDto);
        Task<Guid> CreateEducationLevel(EducationLevelDto educationLevelDto);
        Task DeleteEducationLevel(Guid educationLevelGuid);
    }
}
