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
        Task<BasicResponseDto> UpdateProfileInformationAsync(SubscriberDto Subscriber);
        Task<BasicResponseDto> UpdateOnboardingStatusAsync();
        Task<Guid> EnrollStudentAndObtainEnrollmentGUIDAsync(EnrollmentFlowDto enrollmentFlowDto);
        Task<SubscriberDto> CreateSubscriberAsync();
        Task<WozCourseProgressDto> UpdateStudentCourseProgressAsync(bool FutureSchedule);
        Task<BraintreeResponseDto> SubmitBraintreePaymentAsync(BraintreePaymentDto BraintreePaymentDto);
        Task<IList<CountryDto>> _GetCountriesAsync();
        Task<IList<StateDto>> _GetStatesByCountryAsync(Guid? countryGuid);
        Task<CourseVariantDto> _GetCourseVariantAsync(Guid courseVariantGuid);
        Task<BasicResponseDto> SyncLinkedInAccountAsync(string linkedInCode, string returnUrl);
        Task<IList<SkillDto>> GetSkillsAsync(string userQuery);
        Task<IList<CompanyDto>> GetCompaniesAsync(string userQuery);
        Task<IList<EducationalInstitutionDto>> GetEducationalInstitutionsAsync(string userQuery);
        Task<IList<EducationalDegreeDto>> GetEducationalDegreesAsync(string userQuery);
        Task<IList<CompensationTypeDto>> GetCompensationTypesAsync();
        Task<IList<EducationalDegreeTypeDto>> GetEducationalDegreeTypesAsync();
        Task<IList<SkillDto>> GetSkillsBySubscriberAsync(Guid subscriberGuid);
        Task<BasicResponseDto> UploadResumeAsync(ResumeDto resumeDto);
        Task<SubscriberWorkHistoryDto> AddWorkHistoryAsync(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory);
        Task<SubscriberWorkHistoryDto> UpdateWorkHistoryAsync(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory);
        Task<SubscriberEducationHistoryDto> UpdateEducationHistoryAsync(Guid subscriberGuid, SubscriberEducationHistoryDto educationHistory);
        Task<SubscriberWorkHistoryDto> DeleteWorkHistoryAsync(Guid subscriberGuid, Guid workHistoryGuid);
        Task<SubscriberEducationHistoryDto> DeleteEducationHistoryAsync(Guid subscriberGuid, Guid educationHistory);
        Task<IList<SubscriberWorkHistoryDto>> GetWorkHistoryAsync(Guid subscriberGuid);
        Task<IList<SubscriberEducationHistoryDto>> GetEducationHistoryAsync(Guid subscriberGuid);
        Task<BasicResponseDto> UpdateSubscriberContactAsync(Guid contactGuid, SignUpDto signUpDto);
        Task<BasicResponseDto> ExpressUpdateSubscriberContactAsync(SignUpDto signUpDto);
        Task<CourseDto> GetCourseByCampaignGuidAsync(Guid CampaignGuid);
        Task<SubscriberEducationHistoryDto> AddEducationalHistoryAsync(Guid subscriberGuid, SubscriberEducationHistoryDto workHistory);
        Task<ContactDto> ContactAsync(Guid contactGuid);
        Task<LinkedInProfileDto> GetLinkedInProfileAsync();
        Task<SubscriberADGroupsDto> MyGroupsAsync();
        Task<BasicResponseDto> VerifyEmailAsync(Guid token);

        #region TalentPortal
        Task<IList<SubscriberDto>> SubscriberSearchAsync(string searchQuery);
        #endregion
        #region AdminPortal
        Task<BasicResponseDto> UpdateEntitySkillsAsync(EntitySkillDto entitySkillDto);
        Task<IList<SkillDto>> GetEntitySkillsAsync(string entityType, Guid entityGuid);
        Task<IList<PartnerDto>> GetPartnersAsync();
        Task<PartnerDto> GetPartnerAsync(Guid partnerGuid);
        Task<PartnerDto> CreatePartnerAsync(PartnerDto partnerDto);
        Task<BasicResponseDto> UpdatePartnerAsync(PartnerDto partnerDto);
        Task<BasicResponseDto> DeletePartnerAsync(Guid PartnerGuid);
        Task<List<ImportActionDto>> ImportContactsAsync(Guid partnerGuid, string cacheKey);

        #endregion

        #region Marketing
        Task<IList<CampaignStatisticDto>> CampaignStatisticsSearchAsync();
        Task<IList<CampaignDetailDto>> CampaignDetailsSearchAsync(Guid campaignGuid);
        #endregion

        Task<HttpResponseMessage> DownloadFileAsync(Guid subscriberGuid, Guid fileGuid);
    }
}