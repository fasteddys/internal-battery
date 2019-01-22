using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace UpDiddy.Api
{
    public interface IApi
    { 
        IList<TopicDto> Topics();
        TopicDto TopicById(int TopicId);
        TopicDto TopicBySlug(string TopicSlug);
        IList<CourseDto> getCoursesByTopicSlug(string TopicSlug);
        CourseDto Course(string CourseSlug);
        CourseDto CourseByGuid(Guid CourseGuid);
        IList<CountryDto> GetCountries();
        IList<StateDto> GetStatesByCountry(Guid? countryGuid);
        CourseVariantDto GetCourseVariant(Guid courseVariantGuid);
        SubscriberDto Subscriber(Guid subscriberGuid, bool hardRefresh);
        PromoCodeDto PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseGuid);
        PromoCodeDto PromoCodeValidation(string code, string courseVariantGuid);
        CourseLoginDto CourseLogin(Guid EnrollmentGuid);
        BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber);
        BasicResponseDto UpdateOnboardingStatus();
        Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto);
        SubscriberDto CreateSubscriber();
        WozCourseProgressDto UpdateStudentCourseProgress(bool FutureSchedule);
        BraintreeResponseDto SubmitBraintreePayment(BraintreePaymentDto BraintreePaymentDto);
        IList<CountryDto> _GetCountries();
        IList<StateDto> _GetStatesByCountry(Guid? countryGuid);
        CourseVariantDto _GetCourseVariant(Guid courseVariantGuid);
        BasicResponseDto SyncLinkedInAccount(string linkedInCode, string returnUrl);
        IList<SkillDto> GetSkills(string userQuery);
        IList<CompanyDto> GetCompanies(string userQuery);
        IList<EducationalInstitutionDto> GetEducationalInstitutions(string userQuery);
        IList<EducationalDegreeDto> GetEducationalDegrees(string userQuery);
        IList<CompensationTypeDto> GetCompensationTypes();
        IList<EducationalDegreeTypeDto> GetEducationalDegreeTypes();
        IList<SkillDto> GetSkillsBySubscriber(Guid subscriberGuid);
        BasicResponseDto UploadResume(ResumeDto resumeDto);
        SubscriberWorkHistoryDto AddWorkHistory(SubscriberWorkHistoryDto workHistory);
        SubscriberWorkHistoryDto UpdateWorkHistory(SubscriberWorkHistoryDto workHistory);
        SubscriberEducationHistoryDto UpdateEducationHistory(SubscriberEducationHistoryDto educationHistory);
        SubscriberWorkHistoryDto DeleteWorkHistory(Guid workHistoryGuid);
        SubscriberEducationHistoryDto DeleteEducationHistory(Guid educationHistory);
        IList<SubscriberWorkHistoryDto> GetWorkHistory();
        IList<SubscriberEducationHistoryDto> GetEducationHistory();
        SubscriberEducationHistoryDto AddEducationalHistory(SubscriberEducationHistoryDto workHistory);

        SubscriberADGroupsDto MyGroups();

        #region TalentPortal
        IList<SubscriberDto> SubscriberSearch(string searchQuery);
        #endregion
    }
}