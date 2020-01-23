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
        Task<List<RelatedCourseDto>> GetCoursesByCourse(Guid courseGuid, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesByCourses(List<Guid> courseGuids, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesByJob(Guid jobPostingGuid, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesByJobs(List<Guid> jobPostingGuids, int limit, int offset);
        Task<List<RelatedCourseDto>> GetCoursesBySubscriber(Guid subscriberGuid, int limit, int offset);
        Task<List<RelatedJobDto>> GetJobsByCourse(Guid courseGuid, int limit, int offset, Guid? subscriberGuid = null);
        Task<List<RelatedJobDto>> GetJobsByCourses(List<Guid> courseGuids, int limit, int offset, Guid? subscriberGuid = null);
        Task<List<RelatedJobDto>> GetJobsByTopic(Guid topicGuid, int limit, int offset, Guid? subscriberGuid = null);
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
        Task<List<CourseDetailDto>> GetCoursesRandom(int NumCourses);
        Task<List<CourseDetailDto>> GetCourses(int limit, int offset, string sort, string order);    
        Task<CourseDetailDto> GetCourse(Guid courseGuid);
        Task<List<CourseFavoriteDto>> GetFavoriteCourses(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<SubscriberNotesDto>> GetSubscriberNotes(Guid subscriberGuid, Guid talentGuid, int limit, int offset, string sort, string order);
        Task<List<SubscriberCourseDto>> GetSubscriberCourses(Guid subscriberGuid, int excludeCompleted, int excludeActive);
        Task<List<JobSitemapDto>> GetJobSitemapUrls(Uri baseSiteUri);
        Task<List<CourseVariantDetailDto>> GetCourseVariants(Guid courseGuid);
        [Obsolete("Remove this after the legacy admin portal is gone.")]
        Task<List<UpDiddyLib.Dto.NotificationDto>> GetLegacyNotifications(int limit, int offset, string sort, string order);
        [Obsolete("Remove this after the legacy admin portal is gone.")]
        Task<List<UpDiddyLib.Dto.NotificationDto>> GetLegacySubscriberNotifications(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.NotificationDto>> GetNotifications(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.NotificationDto>> GetSubscriberNotifications(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<CompanyDto>> GetCompanies(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.TopicDto>> GetTopics(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.CompensationTypeDto>> GetCompensationTypes(int limit, int offset, string sort, string order);
        Task<List<CountryDetailDto>> GetCountries(int limit, int offset, string sort, string order);
        Task<List<StateDetailDto>> GetStates(Guid country, int limit, int offset, string sort, string order);
        Task<List<CourseLevelDto>> GetCourseLevels(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.EducationLevelDto>> GetEducationLevels(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.EducationalDegreeTypeDto>> GetEducationalDegreeTypes(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.EmploymentTypeDto>> GetEmploymentTypes(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.ExperienceLevelDto>> GetExperienceLevels(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.IndustryDto>> GetIndustries(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.OfferDto>> GetOffers(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.SecurityClearanceDto>> GetSecurityClearances(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetSkills(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.TalentFavoriteDto>> GetTalentFavorites(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<SubscriberNotesDto>> GetSubscriberNotes(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<CourseDetailDto>> GetCoursesByTopic(Guid topic, int limit, int offset, string sort, string order);
        Task<List<JobCrudDto>> GetSubscriberJobPostingCruds(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.PartnerDto>> GetPartners(int limit, int offset, string sort, string order);

        Task<int> UpdateNotificationCoursesAsync(Guid subscriberGuid, Guid notificationGuid, List<Guid> groups);

        Task<List<GroupInfoDto>> GetGroups(int limit, int offset, string sort, string order);
        Task<List<RecruiterInfoDto>> GetRecruiters(int limit, int offset, string sort, string order);
    }
}