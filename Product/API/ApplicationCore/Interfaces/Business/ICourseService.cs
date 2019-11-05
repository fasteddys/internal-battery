using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetCoursesAsync();
        Task<int> AddCourseAsync(CourseDto courseDto);
        Task<int> EditCourseAsync(CourseDto courseDto);
        Task DeleteCourseAsync(Guid courseGuid);

        Task<List<CourseDetailDto>> GetCoursesForJob(Guid jobGuid, IQueryCollection Query);
        Task<List<CourseDetailDto>> GetCoursesBySkillHistogram(Dictionary<string, int> SkillHistogram, IQueryCollection query);
        Task<List<CourseDetailDto>> GetCoursesRandom(IQueryCollection query);
    }
}
