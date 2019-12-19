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
        Task<List<RelatedCourseDto>> GetCoursesByCourses(List<Guid> courseGuids, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesByCourse(Guid courseGuid, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesBySubscriber(Guid subscriberGuid, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesByJobs(List<Guid> jobPostingGuids, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesByJob(Guid jobPostingGuid, int limit, int offset);
        Task<int> AddCourseAsync(CourseDto courseDto);
        Task<int> EditCourseAsync(CourseDto courseDto);
        Task DeleteCourseAsync(Guid courseGuid);
        Task<List<CourseDetailDto>> GetCoursesRandom(IQueryCollection query);
        Task<List<CourseDetailDto>> GetCourses(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<CourseDetailDto> GetCourse(Guid course);

        Task<int> GetCoursesCount();

        Task<List<CourseDetailDto>> SearchCoursesAsync(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending", string keyword = "*");
        Task<List<CourseVariantDetailDto>> GetCourseVariants(Guid courseGuid);
    }
}
