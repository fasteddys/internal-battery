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

            GetSubscriber();
            
            CourseDto Course = API.Course(CourseSlug);
            TopicDto ParentTopic = API.TopicById(Course.TopicId);
            WozTermsOfServiceDto WozTOS = API.GetWozTermsOfService();
            CourseViewModel CourseViewModel = new CourseViewModel(_configuration, Course, this.subscriber, ParentTopic, WozTOS);
            var gateway = braintreeConfiguration.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View("Checkout", CourseViewModel);
        }

        [HttpGet]
        [Route("/Course/PromoCode/{CourseGuid}/{PromotionalCode}")]
        public string PromoCode(Guid CourseGuid, string PromotionalCode)
        {
            CourseDto Course = API.CourseByGuid(CourseGuid);
            PromoCodeDto Code = API.GetPromoCode(PromotionalCode);
            return AssemblePromoCodeJSONResponse(Code, Course);
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
            string PromoCodeForSubmission)
        {          
            GetSubscriber();
            DateTime dateTime = new DateTime();
            CourseDto Course = API.Course(CourseSlug);
            PromoCodeDto Code = API.GetPromoCode(PromoCodeForSubmission);
            EnrollmentDto enrollmentDto = new EnrollmentDto
            {
                CourseId = Course.CourseId,
                EnrollmentGuid = Guid.NewGuid(),
                SubscriberId = this.subscriber.SubscriberId,
                DateEnrolled = dateTime,
                PricePaid = (decimal)Course.Price,
                PercentComplete = 0,
                IsRetake = 0, //TODO Make this check DB for existing entry
                EnrollmentStatusId = 0,
                TermsOfServiceFlag = TermsOfServiceDocId
            };
            API.EnrollStudentAndObtainEnrollmentGUID(enrollmentDto);
            TopicDto ParentTopic = API.TopicById(Course.TopicId);
            WozTermsOfServiceDto WozTOS = API.GetWozTermsOfService();
            CourseViewModel CourseViewModel = new CourseViewModel(_configuration, Course, this.subscriber, ParentTopic, WozTOS);
            BraintreePaymentDto BraintreePaymentDto = new BraintreePaymentDto
            {
                PaymentAmount = (Decimal)Course.Price,
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
                MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("BraintreeMerchantAccountID"),
                PromoCodeForSubmission = "This will be the promocode from Bill"
            };

            BraintreeResponseDto brdto = API.SubmitBraintreePayment(BraintreePaymentDto);
            if (brdto.WasSuccessful)
            {
                return View("EnrollmentSuccess", CourseViewModel);
            }
            else
            {
                return View("EnrollmentFailure", CourseViewModel);
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


        private string AssemblePromoCodeJSONResponse(PromoCodeDto code, CourseDto course)
        {
            StringBuilder jsonString = new StringBuilder("{");
            jsonString.Append("\"PromoValueFactor\": \"" + code.PromoValueFactor + "\",");
            jsonString.Append("\"AmountOffCourse\": \"" + CalculatePriceOffOfCourse(course.Price, code.PromoValueFactor) + "\",");
            jsonString.Append("\"NewCoursePrice\": \"" + CalculatePromoCodeDiscountedPrice(course.Price, code.PromoValueFactor) + "\"");
            jsonString.Append("}");
            return jsonString.ToString();
        }

        private Decimal? CalculatePromoCodeDiscountedPrice(Decimal? CoursePrice, Decimal PromoValueFactor)
        {
            return (CoursePrice - CalculatePriceOffOfCourse(CoursePrice, PromoValueFactor));
        }

        private Decimal CalculatePriceOffOfCourse(Decimal? CoursePrice, Decimal PromoValueFactor)
        {
            return (Math.Floor((Decimal)CoursePrice * PromoValueFactor * 100)) / 100;
        }
        #endregion




    }
}
