using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers.Braintree;
using UpDiddyLib.Helpers;
using Braintree;
using System.Text;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class CourseController : BaseController
    {
        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IConfiguration _configuration;
        private IBraintreeConfiguration braintreeConfiguration;
        private static readonly TransactionStatus[] transactionSuccessStatuses = {
                TransactionStatus.AUTHORIZED,
                TransactionStatus.AUTHORIZING,
                TransactionStatus.SETTLED,
                TransactionStatus.SETTLING,
                TransactionStatus.SETTLEMENT_CONFIRMED,
                TransactionStatus.SETTLEMENT_PENDING,
                TransactionStatus.SUBMITTED_FOR_SETTLEMENT
            };

        public CourseController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<HomeController> localizer, IConfiguration configuration) : base(azureAdB2COptions.Value, configuration)
        {
            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
            braintreeConfiguration = new BraintreeConfiguration(_configuration);

        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        [Route("/Course/Checkout/{CourseSlug}")]
        public IActionResult Get(string CourseSlug)
        {

            GetSubscriber(false);

            CourseDto Course = API.Course(CourseSlug);
            WozCourseScheduleDto CourseScheduleDto = API.CourseSchedule(Course.Code, (Guid)Course.CourseGuid);
            TopicDto ParentTopic = API.TopicById(Course.TopicId);
            WozTermsOfServiceDto WozTOS = API.GetWozTermsOfService();
            IList<CountryStateDto> CountryStateList = API.GetCountryStateList();
            CourseViewModel CourseViewModel = new CourseViewModel(_configuration, Course, this.subscriber, ParentTopic, CountryStateList, WozTOS, CourseScheduleDto);
            var gateway = braintreeConfiguration.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View("Checkout", CourseViewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("/Course/PromoCodeValidation/{code}/{courseGuid}/{subscriberGuid}")]
        public IActionResult PromoCodeValidation(string code, string courseGuid, string subscriberGuid)
        {
            PromoCodeDto promoCodeDto = API.PromoCodeValidation(code, courseGuid, subscriberGuid);
            return new ObjectResult(promoCodeDto);
        }

        [HttpPost]
        public IActionResult Checkout(
            int TermsOfServiceDocId,
            string CourseSlug,
            Boolean SameAsAboveCheckbox,
            string BillingFirstName,
            string BillingLastName,
            string BillingZipCode,
            string BillingState,
            string BillingCity,
            string BillingCountry,
            string BillingAddress,
            Guid? PromoCodeRedemptionGuid,
            string sectionSelectionRadios,
            Int64 DateOfInstructorLedSection,
            string SubscriberFirstName,
            string SubscriberLastName)
        {
            GetSubscriber(false);

            // update the subscriber's first name and last name if they do not exist (or if the values are different than what is stored currently). 
            // we must do this here because first and last name are required for the enrollment process for Woz. 
            // todo: refactor this; consider creating a partial view that is shared between the profile page, checkout, and anywhere else where we need to work with the subscriber
            if (this.subscriber.FirstName == null || !this.subscriber.FirstName.Equals(SubscriberFirstName) || this.subscriber.LastName == null || !this.subscriber.LastName.Equals(SubscriberLastName))
            {
                API.UpdateProfileInformation(new SubscriberDto()
                {
                    SubscriberGuid = this.subscriber.SubscriberGuid,
                    FirstName = SubscriberFirstName,
                    LastName = SubscriberLastName
                });
            }

            DateTime currentDate = DateTime.UtcNow;
            CourseDto Course = API.Course(CourseSlug);
            WozCourseScheduleDto wcsdto = API.CourseSchedule(Course.Code, (Guid)Course.CourseGuid);
            TopicDto ParentTopic = API.TopicById(Course.TopicId);
            WozTermsOfServiceDto WozTOS = API.GetWozTermsOfService();
            IList<CountryStateDto> CountryStateList = API.GetCountryStateList();
            CourseViewModel CourseViewModel = new CourseViewModel(_configuration, Course, this.subscriber, ParentTopic, CountryStateList, WozTOS, wcsdto);

            EnrollmentDto enrollmentDto = new EnrollmentDto
            {
                CreateDate = currentDate,
                ModifyDate = currentDate,
                DateEnrolled = currentDate,
                CreateGuid = Guid.NewGuid(),
                ModifyGuid = Guid.NewGuid(),
                CourseId = Course.CourseId,
                EnrollmentGuid = Guid.NewGuid(),
                SubscriberId = this.subscriber.SubscriberId,
                PricePaid = (decimal)Course.Price,
                PercentComplete = 0,
                IsRetake = 0, //TODO Make this check DB for existing entry=
                EnrollmentStatusId = sectionSelectionRadios == "instructorLed" ? (int)EnrollmentStatus.FutureRegisterStudentRequested : (int)EnrollmentStatus.EnrollStudentRequested,
                SectionStartTimestamp = sectionSelectionRadios == "instructorLed" ? DateOfInstructorLedSection : 0,
                TermsOfServiceFlag = TermsOfServiceDocId ,
                Subscriber = this.subscriber,
                // todo: refactor course variant code in model, dto, viewmodel, and ui. it hurts my soul to leave it in this state but there is literally no time
                 CourseVariantId = (sectionSelectionRadios == "instructorLed") ? CourseViewModel.InstructorLedCourseVariantId : CourseViewModel.SelfPacedCourseVariantId
            };

            PromoCodeDto validPromoCode = null;
            // validate, consume, and apply promo code redemption 
            if (PromoCodeRedemptionGuid.HasValue && PromoCodeRedemptionGuid.Value != Guid.Empty)
            {
                validPromoCode = API.PromoCodeRedemptionValidation(PromoCodeRedemptionGuid.Value.ToString(), Course.CourseGuid.Value.ToString(), this.subscriber.SubscriberGuid.Value.ToString());

                if (validPromoCode == null)
                    return View("EnrollmentFailure", CourseViewModel);
                else
                {
                    enrollmentDto.PricePaid -= validPromoCode.Discount;
                    enrollmentDto.PromoCodeRedemptionGuid = validPromoCode.PromoCodeRedemptionGuid;
                }
            }

            /**
             *      TODO: Need to implement logic here to ensure billing fields are
             *      not empty to ensure Braintree gets the values.
             * */

            // process payment if price is not zero
            if (enrollmentDto.PricePaid != 0)
            {
                
                BraintreePaymentDto BraintreePaymentDto = new BraintreePaymentDto
                {
                    PaymentAmount = enrollmentDto.PricePaid,
                    Nonce = Request.Form["payment_method_nonce"],
                    FirstName = BillingFirstName,
                    LastName = BillingLastName,
                    PhoneNumber = this.subscriber.PhoneNumber,
                    Email = this.subscriber.Email,
                    Address = BillingAddress,
                    Region = BillingState,
                    Locality = BillingCity,
                    ZipCode = BillingZipCode,
                    CountryCode = BillingCountry,
                    MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("Braintree:MerchantAccountID")
                };
                EnrollmentFlowDto enrollmentFlowDto = new EnrollmentFlowDto
                {
                    EnrollmentDto = enrollmentDto,
                    BraintreePaymentDto = BraintreePaymentDto,
                    SubscriberDto = this.subscriber
                };

                API.EnrollStudentAndObtainEnrollmentGUID(enrollmentFlowDto);
                return View("EnrollmentSuccess", CourseViewModel);
            }
            else
            {
                // course is free with promo code
                BraintreePaymentDto BraintreePaymentDto = new BraintreePaymentDto
                {
                    PaymentAmount = 0
                };
                EnrollmentFlowDto enrollmentFlowDto = new EnrollmentFlowDto
                {
                    EnrollmentDto = enrollmentDto,
                    BraintreePaymentDto = BraintreePaymentDto
                };
                API.EnrollStudentAndObtainEnrollmentGUID(enrollmentFlowDto);
                return View("EnrollmentSuccess", CourseViewModel);
            }
            // TODO: billing form field validtion using EnsureFormFieldsNotNullOrEmpty method
        }


        public IActionResult EnrollmentSuccess()
        {
            return View();
        }


        #region Private Helpers

        private Boolean EnsureFormFieldsNotNullOrEmpty(
            string BillingFirstName,
            string BillingLastName,
            string BillingZipCode,
            string BillingCity,
            string BillingState,
            string BillingCountry,
            string BillingAddress)
        {
            if (BillingFirstName != null && !("").Equals(BillingFirstName)
                && BillingLastName != null && !("").Equals(BillingLastName)
                && BillingZipCode != null && !("").Equals(BillingZipCode)
                && BillingCity != null && !("").Equals(BillingCity)
                && BillingState != null && !("").Equals(BillingState)
                && BillingCountry != null && !("").Equals(BillingCountry)
                && BillingAddress != null && !("").Equals(BillingAddress)
                )
            {
                return true;
            }
            return false;
        }
        #endregion




    }
}
