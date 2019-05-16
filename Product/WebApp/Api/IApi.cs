using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Dto.Reporting;

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
        Task<IList<IndustryDto>> GetIndustryAsync();
        Task<IList<JobCategoryDto>> GetJobCategoryAsync();
        Task<List<JobPostingDto>> GetAllJobsAsync();
        Task<IList<ExperienceLevelDto>> GetExperienceLevelAsync();
        Task<IList<EducationLevelDto>> GetEducationLevelAsync();
        Task<IList<CompensationTypeDto>> GetCompensationTypeAsync();
        Task<IList<EmploymentTypeDto>> GetEmploymentTypeAsync();
        Task<IList<SecurityClearanceDto>> GetSecurityClearanceAsync();
        Task<IList<RecruiterCompanyDto>> GetRecruiterCompaniesAsync(Guid subscriberGuid);
        Task<CourseVariantDto> GetCourseVariantAsync(Guid courseVariantGuid);
        Task<SubscriberDto> SubscriberAsync(Guid subscriberGuid, bool hardRefresh);
        Task<PromoCodeDto> PromoCodeRedemptionValidationAsync(string promoCodeRedemptionGuid, string courseGuid);
        Task<PromoCodeDto> PromoCodeValidationAsync(string code, string courseVariantGuid);
        Task<CourseLoginDto> CourseLoginAsync(Guid EnrollmentGuid);
        Task<BasicResponseDto> UpdateProfileInformationAsync(SubscriberDto Subscriber);
        Task<BasicResponseDto> UpdateOnboardingStatusAsync();
        Task<Guid> EnrollStudentAndObtainEnrollmentGUIDAsync(EnrollmentFlowDto enrollmentFlowDto);
        Task<SubscriberDto> CreateSubscriberAsync();
        Task<bool> DeleteSubscriberAsync(Guid subscriberGuid);
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
        Task<BasicResponseDto> UpdateSubscriberContactAsync(Guid partnerContactGuid, SignUpDto signUpDto);
        Task<BasicResponseDto> ExpressUpdateSubscriberContactAsync(SignUpDto signUpDto);
        Task<CourseDto> GetCourseByCampaignGuidAsync(Guid CampaignGuid);
        Task<SubscriberEducationHistoryDto> AddEducationalHistoryAsync(Guid subscriberGuid, SubscriberEducationHistoryDto workHistory);
        Task<BasicResponseDto> AddJobPostingAsync(JobPostingDto jobPosting);
        Task<BasicResponseDto> UpdateJobPostingAsync(JobPostingDto jobPosting);

        Task<List<JobPostingDto>> GetJobPostingsForSubscriber(Guid subscriberGuid);
        Task<JobPostingDto> GetJobPostingByGuid(Guid jobPostingGuid);

        Task<JobPostingDto> CopyJobPosting(Guid jobPostingGuid);

        Task<BasicResponseDto> DeleteJobPosting(Guid jobPostingGuid);

        Task<ContactDto> ContactAsync(Guid partnerContactGuid);
        Task<LinkedInProfileDto> GetLinkedInProfileAsync();
        Task<SubscriberADGroupsDto> MyGroupsAsync();
        Task<BasicResponseDto> VerifyEmailAsync(Guid token);
        Task<IList<CampaignDto>> GetCampaignsAsync();
        Task<CampaignDto> GetCampaignAsync(Guid campaignGuid);
        Task<CampaignPartnerContactDto> GetCampaignPartnerContactAsync(string tinyId);
        Task<IList<OfferDto>> GetOffersAsync();

        #region TalentPortal
        Task<IList<SubscriberDto>> SubscriberSearchAsync(string searchFilter, string searchQuery);
        Task<IList<SubscriberSourceDto>> SubscriberSourcesAsync();
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
        Task<IList<JobSiteScrapeStatisticDto>> JobScrapeStatisticsSearchAsync(int numRecords);


        #endregion

        #region Marketing
        Task<IList<CampaignStatisticDto>> CampaignStatisticsSearchAsync();
        Task<IList<CampaignDetailDto>> CampaignDetailsSearchAsync(Guid campaignGuid);
        #endregion

        #region Reporting
        Task<SubscriberReportDto> GetSubscriberReportAsync(List<DateTime> dates = null);
        Task<SubscriberReportDto> GetSubscriberReportByPartnerAsync();
        Task<List<RecruiterActionSummaryDto>> GetRecruiterActionSummaryAsync();
        Task<List<SubscriberActionSummaryDto>> GetSubscriberActionSummaryAsync();
        Task<List<OfferActionSummaryDto>> GetOfferActionSummaryAsync();
        Task<ActionReportDto> GetPartnerSubscriberActionStatsAsync();
        Task<List<JobApplicationCountDto>> GetJobApplicationCount(Guid? companyGuid = null);
        #endregion

        #region JobBoard

        Task<JobPostingDto> GetJobAsync(Guid JobPostingGuid, GoogleCloudEventsTrackingDto dto = null);
        Task<JobPostingDto> GetExpiredJobAsync(Guid JobPostingGuid);
        Task<BasicResponseDto> ApplyToJobAsync(JobApplicationDto JobApplication);
        Task<JobSearchResultDto> GetJobsByLocation(string keywords, string location);

        Task<GoogleCloudEventsTrackingDto> RecordClientEventAsync(Guid jobGuid, GoogleCloudEventsTrackingDto dto);

        #endregion

        Task<HttpResponseMessage> DownloadFileAsync(Guid subscriberGuid, Guid fileGuid);
    }
}