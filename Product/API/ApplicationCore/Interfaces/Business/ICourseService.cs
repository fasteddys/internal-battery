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
    }
}
