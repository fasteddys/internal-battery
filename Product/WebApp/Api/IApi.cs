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
        SubscriberDto Subscriber();
        PromoCodeDto PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseGuid);
        PromoCodeDto PromoCodeValidation(string code, string courseVariantGuid);
        CourseLoginDto CourseLogin(Guid EnrollmentGuid);
        BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber);
        BasicResponseDto UpdateOnboardingStatus(Guid SubscriberGuid);
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
        IList<CompensationTypeDto> GetCompensationTypes();
        IList<SkillDto> GetSkillsBySubscriber(Guid subscriberGuid);
        BasicResponseDto UploadResume(ResumeDto resumeDto);
        SubscriberWorkHistoryDto AddWorkHistory(SubscriberWorkHistoryDto workHistory);
        SubscriberWorkHistoryDto UpdateWorkHistory(SubscriberWorkHistoryDto workHistory);
        IList<SubscriberWorkHistoryDto> GetWorkHistory();
    }
}