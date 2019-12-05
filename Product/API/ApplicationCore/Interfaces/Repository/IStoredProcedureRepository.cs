using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IStoredProcedureRepository
    {
        Task<List<RelatedJobDto>> GetJobsByCourse(Guid courseGuid, int limit, int offset, Guid? subscriberGuid = null);
        Task<List<RelatedJobDto>> GetJobsBySubscriber(Guid subscriberGuid, int limit, int offset);
        Task CacheRelatedJobSkillMatrix();
        Task<List<SearchTermDto>> GetKeywordSearchTermsAsync();
        Task<List<SearchTermDto>> GetLocationSearchTermsAsync();
        Task<List<JobAbandonmentStatistics>> GetJobAbandonmentStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<List<JobCountPerProvince>> GetJobCountPerProvince();
        Task<List<JobDto>> GetSubscriberJobFavorites(int SubscriberId);
        Task<List<SubscriberSourceDto>> GetSubscriberSources(int SubscriberId);
        Task<List<SubscriberSignUpCourseEnrollmentStatistics>> GetSubscriberSignUpCourseEnrollmentStatisticsAsync(DateTime? startDate, DateTime? endDate);
        Task<int> AddOrUpdateCourseAsync(CourseParams courseParams);
        Task<List<SubscriberInitialSourceDto>> GetNewSubscribers();
        Task<List<CourseDetailDto>> GetCoursesForJob(Guid JobGuid, int NumCourses);
        Task<List<CourseDetailDto>> GetCoursesBySkillHistogram(Dictionary<string, int> SkillHistogram, int NumCourses);
        Task<List<CourseDetailDto>> GetCoursesRandom(int NumCourses);
        Task<List<CourseDetailDto>> GetCourses(int limit, int offset, string sort, string order);
        Task<CourseDetailDto> GetCourse(Guid courseGuid);
        Task<List<CourseDetailDto>> GetFavoriteCourses(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<SubscriberNotesDto>> GetSubscriberNotes(Guid subscriberGuid, Guid talentGuid, int limit, int offset, string sort, string order);
    }
}