using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;
using System.Security.Claims;
using UpDiddy.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using UpDiddy.Helpers;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using UpDiddy.Helpers.Braintree;
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

            GetSubscriber(false);
            
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
            GetSubscriber(false);
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




            // -----------------------  Braintree Integration  ------------------------------

            // TODO: billing form field validtion using EnsureFormFieldsNotNullOrEmpty method


            var gateway = braintreeConfiguration.GetGateway();
            Decimal amount;
            if (PromoCodeForSubmission != null) {
                amount = (Decimal)CalculatePromoCodeDiscountedPrice(Course.Price, Code.PromoValueFactor);
            }
            else {
                amount = Convert.ToDecimal(Course.Price);
            }

            var nonce = Request.Form["payment_method_nonce"];
            
            AddressRequest addressRequest;

            if (SameAsAboveCheckbox)
            {
                addressRequest = new AddressRequest
                {
                    FirstName = this.subscriber.FirstName,
                    LastName = this.subscriber.LastName,
                    StreetAddress = this.subscriber.Address
                };
            }
            else
            {
                addressRequest = new AddressRequest
                {
                    // Assuming form fields are filled in at this point until above TODO is handled
                    FirstName = BillingFirstName,
                    LastName = BillingLastName,
                    StreetAddress = BillingAddress,
                    Region = BillingState,
                    Locality = BillingCity,
                    PostalCode = BillingZipCode,
                    CountryCodeAlpha2 = BillingCountry

                };
            }
            
            TransactionRequest request = new TransactionRequest
            {
                Amount = amount,
                MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("BraintreeMerchantAccountID"),
                PaymentMethodNonce = nonce,
                Customer = new CustomerRequest
                {
                    FirstName = this.subscriber.FirstName,
                    LastName = this.subscriber.LastName,
                    Phone = this.subscriber.PhoneNumber,
                    Email = this.subscriber.Email
                },
                BillingAddress = addressRequest,
                ShippingAddress = new AddressRequest
                {
                    FirstName = this.subscriber.FirstName,
                    LastName = this.subscriber.LastName,
                    StreetAddress = this.subscriber.Address
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
            };
            
            Result<Transaction> result = gateway.Transaction.Sale(request);
            
            if (result.IsSuccess())
            {
                Transaction transaction = result.Target;
                return View("EnrollmentSuccess", CourseViewModel);
            }
            else
            {
                return View("EnrollmentFailure", CourseViewModel);
            }
            
        }

        public Boolean EnsureFormFieldsNotNullOrEmpty(
            string BillingFirstName,
            string BillingLastName,
            string BillingZipCode,
            string BillingCity,
            string BillingState,
            string BillingCountry,
            string BillingAddress)
        {
            if(BillingFirstName != null && !("").Equals(BillingFirstName)
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

        public IActionResult EnrollmentSuccess()
        {
            return View();
        }



    }
}
