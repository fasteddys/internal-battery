using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Reports;
using UpDiddyLib.Domain.AzureSearchDocuments;

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
        Task UpdateEntitySkills(Guid entityGuid, string entityType, List<Guid> skillGuids);
        Task<List<GroupInfoDto>> GetGroups(int limit, int offset, string sort, string order);
        Task<List<RecruiterInfoDto>> GetRecruiters(int limit, int offset, string sort, string order);
        Task<List<UpDiddyLib.Domain.Models.JobSiteScrapeStatisticDto>> GetJobSiteScrapeStatistics(int limit, int offset, string sort, string order);
        Task<List<UsersDto>> GetNewUsers();
        Task<List<UsersDetailDto>> GetAllUsersDetail();
        Task<List<UsersDetailDto>> GetAllHiringManagersDetail();
        Task<List<UsersDetailDto>> GetUsersByPartnerDetail(Guid partner, DateTime startDate, DateTime endDate);
        Task<List<PartnerUsers>> GetUsersByPartner(DateTime startDate, DateTime endDate);
        Task<bool> InsertSendGridEvents(string sendGridJson);
        Task<bool> PurgeSendGridEvents(int LookbackDays);
        Task<List<SubscriberEmailStatisticDto>> GetSubscriberEmailStatistics(string email);
        Task<Tuple<Guid?, string>> CreateJobPosting(JobCrudDto jobCrudDto);
        Task<string> UpdateJobPosting(JobCrudDto jobCrudDto);
        Task UpdateJobPostingSkills(Guid jobPostingGuid, List<Guid> skillGuids);
        Task<int> CreateSubscriberG2Profiles(Guid subscriberGuid);
        Task<int> DeleteSubscriberG2Profiles(Guid subscriberGuid);
        Task<int> CreateCompanyG2Profiles(Guid companyGuid);
        Task<int> DeleteCompanyG2Profiles(Guid companyGuid);
        Task<bool> UpdateG2AzureIndexStatuses(List<AzureIndexResultStatus> profileGuids, string statusName, string statusInfo); 
        Task<int> BootG2Profiles();
 

    }  
}