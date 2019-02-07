using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using UpDiddyLib.Dto.Marketing;

namespace UpDiddy.Api
{
    public interface IApi
    { 
        Task<IList<TopicDto>> TopicsAsync();
        Task<TopicDto> TopicByIdAsync(int TopicId);
        Task<TopicDto> TopicBySlugAsync(string TopicSlug);
        Task<IList<CourseDto>> getCoursesByTopicSlugAsync(string TopicSlug);
        Task<CourseDto> CourseAsync(string CourseSlug);
        Task<IList<CourseDto>> CoursesAsync();
        Task<IList<CountryDto>> GetCountriesAsync();
        Task<IList<StateDto>> GetStatesByCountryAsync(Guid? countryGuid);
        Task<CourseVariantDto> GetCourseVariantAsync(Guid courseVariantGuid);
        Task<SubscriberDto> SubscriberAsync(Guid subscriberGuid, bool hardRefresh);
        Task<PromoCodeDto> PromoCodeRedemptionValidationAsync(string promoCodeRedemptionGuid, string courseGuid);
        Task<PromoCodeDto> PromoCodeValidationAsync(string code, string courseVariantGuid);
        Task<CourseLoginDto> CourseLoginAsync(Guid EnrollmentGuid);
        BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber);
        BasicResponseDto UpdateOnboardingStatus();
        Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto);
        SubscriberDto CreateSubscriber();
        WozCourseProgressDto UpdateStudentCourseProgress(bool FutureSchedule);
        BraintreeResponseDto SubmitBraintreePayment(BraintreePaymentDto BraintreePaymentDto);
        Task<IList<CountryDto>> _GetCountriesAsync();
        Task<IList<StateDto>> _GetStatesByCountryAsync(Guid? countryGuid);
        Task<CourseVariantDto> _GetCourseVariantAsync(Guid courseVariantGuid);
        BasicResponseDto SyncLinkedInAccount(string linkedInCode, string returnUrl);
        Task<IList<SkillDto>> GetSkillsAsync(string userQuery);
        Task<IList<CompanyDto>> GetCompaniesAsync(string userQuery);
        Task<IList<EducationalInstitutionDto>> GetEducationalInstitutionsAsync(string userQuery);
        Task<IList<EducationalDegreeDto>> GetEducationalDegreesAsync(string userQuery);
        Task<IList<CompensationTypeDto>> GetCompensationTypesAsync();
        Task<IList<EducationalDegreeTypeDto>> GetEducationalDegreeTypesAsync();
        Task<IList<SkillDto>> GetSkillsBySubscriberAsync(Guid subscriberGuid);
        BasicResponseDto UploadResume(ResumeDto resumeDto);
        SubscriberWorkHistoryDto AddWorkHistory(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory);
        SubscriberWorkHistoryDto UpdateWorkHistory(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory);
        SubscriberEducationHistoryDto UpdateEducationHistory(Guid subscriberGuid, SubscriberEducationHistoryDto educationHistory);
        SubscriberWorkHistoryDto DeleteWorkHistory(Guid subscriberGuid, Guid workHistoryGuid);
        SubscriberEducationHistoryDto DeleteEducationHistory(Guid subscriberGuid, Guid educationHistory);
        Task<IList<SubscriberWorkHistoryDto>> GetWorkHistoryAsync(Guid subscriberGuid);
        Task<IList<SubscriberEducationHistoryDto>> GetEducationHistoryAsync(Guid subscriberGuid);
        SubscriberEducationHistoryDto AddEducationalHistory(Guid subscriberGuid, SubscriberEducationHistoryDto workHistory);
        BasicResponseDto UpdateSubscriberContact(Guid contactGuid, SignUpDto signUpDto);
        CourseDto GetCourseByCampaignGuid(Guid CampaignGuid);


        Task<SubscriberADGroupsDto> MyGroupsAsync();

        #region TalentPortal
        Task<IList<SubscriberDto>> SubscriberSearchAsync(string searchQuery);
        #endregion
        #region AdminPortal
        BasicResponseDto UpdateEntitySkills(EntitySkillDto entitySkillDto);
        Task<IList<SkillDto>> GetEntitySkillsAsync(string entityType, Guid entityGuid);
        #endregion

        Task<HttpResponseMessage> DownloadFileAsync(Guid subscriberGuid, int fileId);
    }
}