using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.CourseDataMining.Common
{
    public interface ICourseProcess
    {
        List<CoursePage> DiscoverCoursePages(List<CoursePage> existingCoursePages);
        CourseDto ProcessCoursePage(CoursePage coursePage);
    }
}
