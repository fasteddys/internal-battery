using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICareerPathService
    {
       Task<List<CareerPathDto>> GetCareerPaths(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
       Task<List<CourseDetailDto>> GetCareerPathCourses(Guid careerPathGuid);
       Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetCareerPathSkills(Guid careerPathGuid);
    }
}