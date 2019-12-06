using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IExperienceLevelService
    {
        Task<ExperienceLevelDto> GetExperienceLevel(Guid experienceLevelGuid);
        Task<List<ExperienceLevelDto>> GetExperienceLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateExperienceLevel(Guid experienceLevelGuid, ExperienceLevelDto experienceLevelDto);
        Task CreateExperienceLevel(ExperienceLevelDto experienceLevelDto);
        Task DeleteExperienceLevel(Guid experienceLevelGuid);
    }
}
