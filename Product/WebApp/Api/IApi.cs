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
        IList<CourseDto> getCousesByTopicSlug(string TopicSlug);
        CourseDto Course(string CourseSlug);
        CourseDto CourseByGuid(Guid CourseGuid);
        WozTermsOfServiceDto GetWozTermsOfService();
        WozCourseProgressDto GetCurrentCourseProgress(Guid SubscriberGuid, Guid EnrollmentGuid);
        IList<CountryDto> GetCountries();
        IList<StateDto> GetStatesByCountry(Guid? countryGuid);
        CourseVariantDto GetCourseVariant(Guid courseVariantGuid);
        SubscriberDto Subscriber(Guid SubscriberGuid);
        PromoCodeDto PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseGuid, string subscriberGuid);
        PromoCodeDto PromoCodeValidation(string code, string courseVariantGuid, string subscriberGuid);
        VendorStudentLoginDto StudentLogin(int SubscriberId);
        CourseLoginDto CourseLogin(Guid SubscriberGuid, Guid EnrollmentGuid);
        BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber);
        Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto);
        Guid WriteToEnrollmentLog(EnrollmentLogDto enrollmentLogDto);
        SubscriberDto CreateSubscriber(string SubscriberGuid, string SubscriberEmail);
        WozCourseProgressDto UpdateStudentCourseProgress(Guid SubscriberGuid, bool FutureSchedule);
        BraintreeResponseDto SubmitBraintreePayment(BraintreePaymentDto BraintreePaymentDto);
        IList<CountryDto> _GetCountries();
        IList<StateDto> _GetStatesByCountry(Guid? countryGuid);
        CourseVariantDto _GetCourseVariant(Guid courseVariantGuid);
        BasicResponseDto SyncLinkedInAccount(Guid SubscriberGuid, string linkedInCode, string returnUrl);
    }
}