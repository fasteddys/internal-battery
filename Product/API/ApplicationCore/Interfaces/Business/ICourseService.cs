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
        Task AddCourseAsync(CourseDto courseDto);
        Task EditCourseAsync(CourseDto courseDto);
        Task DeleteCourseAsync(Guid courseGuid);
    }
}
