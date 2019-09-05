using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common
{
    public interface ICourseProcess
    {
        Task<List<CoursePage>> DiscoverCoursePagesAsync(List<CoursePage> existingCoursePages);
        Task<CourseDto> ProcessCoursePageAsync(CoursePage coursePage);
    }
}