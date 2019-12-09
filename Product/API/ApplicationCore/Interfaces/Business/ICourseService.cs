using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICourseService
    {
        Task<List<RelatedCourseDto>> GetCoursesByCourse(Guid courseGuid, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesBySubscriber(Guid subscriberGuid, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesByJob(Guid subscriberGuid, int limit, int offset);
        Task<int> AddCourseAsync(CourseDto courseDto);
        Task<int> EditCourseAsync(CourseDto courseDto);
        Task DeleteCourseAsync(Guid courseGuid);
        Task<List<CourseDetailDto>> GetCoursesForJob(Guid jobGuid, IQueryCollection Query);
        Task<List<CourseDetailDto>> GetCoursesBySkillHistogram(Dictionary<string, int> SkillHistogram, IQueryCollection query);
        Task<List<CourseDetailDto>> GetCoursesRandom(IQueryCollection query);
        Task<List<CourseDetailDto>> GetCourses(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<CourseDetailDto> GetCourse(Guid course);
        Task<int> GetCoursesCount();
    }
}
