using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;

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
        Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto);
        Guid WriteToEnrollmentLog(EnrollmentLogDto enrollmentLogDto);
        SubscriberDto CreateSubscriber();
        WozCourseProgressDto UpdateStudentCourseProgress(bool FutureSchedule);
        BraintreeResponseDto SubmitBraintreePayment(BraintreePaymentDto BraintreePaymentDto);
        IList<CountryDto> _GetCountries();
        IList<StateDto> _GetStatesByCountry(Guid? countryGuid);
        CourseVariantDto _GetCourseVariant(Guid courseVariantGuid);
        BasicResponseDto SyncLinkedInAccount(string linkedInCode, string returnUrl);
        IList<SkillDto> GetSkills(string userQuery);
        IList<SkillDto> GetSkillsBySubscriber(Guid subscriberGuid);
    }
}