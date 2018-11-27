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
        WozCourseProgress GetCurrentCourseProgress(Guid SubscriberGuid, Guid EnrollmentGuid);
        IList<CountryDto> GetCountries();
        IList<StateDto> GetStatesByCountry(Guid countryGuid);
        CourseVariantDto GetCourseVariant(Guid courseVariantGuid);
        CountryDto GetSubscriberCountry(int StateId);
        StateDto GetSubscriberState(int StateId);
        SubscriberDto Subscriber(Guid SubscriberGuid);
        PromoCodeDto PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseGuid, string subscriberGuid);
        PromoCodeDto PromoCodeValidation(string code, string courseVariantGuid, string subscriberGuid);
        IList<EnrollmentDto> GetCurrentEnrollmentsForSubscriber(SubscriberDto Subscriber);
        VendorStudentLoginDto StudentLogin(int SubscriberId);
        CourseLoginDto CourseLogin(Guid SubscriberGuid, Guid CourseGuid, Guid VendorGuid);
        BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber);
        Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto);
        Guid WriteToEnrollmentLog(EnrollmentLogDto enrollmentLogDto);
        SubscriberDto CreateSubscriberDeprecated(string SubscriberGuid, string SubscriberEmail);
        SubscriberDto CreateSubscriber(string SubscriberGuid, string SubscriberEmail);
        WozCourseProgress UpdateStudentCourseProgress(Guid SubscriberGuid, bool FutureSchedule);
        BraintreeResponseDto SubmitBraintreePayment(BraintreePaymentDto BraintreePaymentDto);
        IList<CountryDto> _GetCountries();
        IList<StateDto> _GetStatesByCountry(Guid countryGuid);
        CourseVariantDto _GetCourseVariant(Guid courseVariantGuid);
        CountryDto _GetSubscriberCountry(int StateId);
        StateDto _GetSubscriberState(int StateId);
    }
}