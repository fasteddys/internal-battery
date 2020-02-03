using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICourseLevelService
    {
        Task<CourseLevelDto> GetCourseLevel(Guid courseLevelGuid);
        Task<CourseLevelListDto> GetCourseLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task UpdateCourseLevel(Guid courseLevelGuid, CourseLevelDto courseLevelDto);
        Task<Guid> CreateCourseLevel(CourseLevelDto courseLevelDto);
        Task DeleteCourseLevel(Guid courseLevelGuid);
    }
}
