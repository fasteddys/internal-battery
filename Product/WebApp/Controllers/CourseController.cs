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
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        [HttpGet]
        public IActionResult GetCountries()
        {
            return Ok(Json(API.GetCountries()));
        }
        [HttpGet]
        public IActionResult GetStatesByCountry(Guid countryGuid)
        {
            return Ok(Json(API.GetStatesByCountry(countryGuid)));
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("/Course/PromoCodeValidation/{code}/{courseVariantGuid}/{subscriberGuid}")]
        public IActionResult PromoCodeValidation(string code, string courseVariantGuid, string subscriberGuid)
        {
            PromoCodeDto promoCodeDto = API.PromoCodeValidation(code, courseVariantGuid, subscriberGuid);
            return new ObjectResult(promoCodeDto);
        }


        [Authorize]
        [HttpGet]
        [Route("/Course/Checkout/{CourseSlug}")]
        public IActionResult Get(string CourseSlug)
        {

            GetSubscriber(false);

            // todo: this should not be done in the constructor of the view model
            //this.CountryStateMapping = Utils.InitializeCountryStateMapping(CountryStateList);
            //IList<CountryStateDto> CountryStateList = API.GetCountryStateList();

            var gateway = braintreeConfiguration.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            var course = API.Course(CourseSlug);

            var courseVariantViewModels = course.CourseVariants.Select(dto => new CourseVariantViewModel()
            {
                CourseVariantGuid = dto.CourseVariantGuid,
                CourseVariantType = dto.CourseVariantType.Name,
                Price = dto.Price,
                StartDates = dto.StartDateUTCs?.Select(i => new SelectListItem()
                {
                    Text = i.ToShortDateString(),
                    Value = i.ToString()
                })
            });

            CourseViewModel courseViewModel = new CourseViewModel()
            {
                Name = course.Name,
                Description = course.Description,
                Code = course.Code,
                CourseGuid = course.CourseGuid.Value,
                CourseVariants = courseVariantViewModels,
                Course = course, // remove this and replace with only the properties we need to display

                SubscriberFirstName = this.subscriber.FirstName,
                SubscriberLastName = this.subscriber.LastName,
                SubscriberGuid = this.subscriber.SubscriberGuid.Value,


                Countries = API.GetCountries().Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString()
                }),

                States = new List<StateViewModel>().Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.StateGuid.ToString()
                })
            };

            return View("Checkout", courseViewModel);
        }

        [HttpPost]
        public IActionResult Checkout(CourseViewModel courseViewModel)
        {
            GetSubscriber(false);

            // update the subscriber's first name and last name if they do not exist (or if the values are different than what is stored currently). 
            // we must do this here because first and last name are required for the enrollment process for Woz. 
            // todo: refactor this; consider creating a partial view that is shared between the profile page, checkout, and anywhere else where we need to work with the subscriber
            if (this.subscriber.FirstName == null || !this.subscriber.FirstName.Equals(courseViewModel.SubscriberFirstName)
                || this.subscriber.LastName == null || !this.subscriber.LastName.Equals(courseViewModel.SubscriberLastName))
            {
                API.UpdateProfileInformation(new SubscriberDto()
                {
                    SubscriberGuid = courseViewModel.SubscriberGuid,
                    FirstName = courseViewModel.SubscriberFirstName,
                    LastName = courseViewModel.SubscriberLastName
                });
            }

            DateTime currentDate = DateTime.UtcNow;
            CourseDto Course = API.Course(courseViewModel.Course.Slug);

            // use course variant type to infer enrollment status and start date
            int enrollmentStatusId = 0;
            long? sectionStartTimestamp = null;
            switch (courseViewModel.SelectedCourseVariant.CourseVariantType)
            {
                case "Instructor-Led":
                    enrollmentStatusId = (int)EnrollmentStatus.FutureRegisterStudentRequested;
                    if (courseViewModel.SelectedCourseVariant.SelectedStartDate.HasValue)
                        sectionStartTimestamp = Utils.ToWozTime(courseViewModel.SelectedCourseVariant.SelectedStartDate.Value);
                    break;
                case "Self-Paced":
                    enrollmentStatusId = (int)EnrollmentStatus.RegisterStudentRequested;
                    break;
                default:
                    throw new ApplicationException("Unrecognized course variant; cannot determine enrollment status!");
            }

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
                PricePaid = 0, // replace with course variant price - was: (decimal)Course.Price,
                PercentComplete = 0,
                IsRetake = 0, //TODO Make this check DB for existing entry=
                EnrollmentStatusId = enrollmentStatusId,
                SectionStartTimestamp = sectionStartTimestamp,
                TermsOfServiceFlag = courseViewModel.TermsOfServiceDocumentId,
                Subscriber = this.subscriber
                // todo: refactor course variant code in model, dto, viewmodel, and ui. it hurts my soul to leave it in this state but there is literally no time
                //CourseVariantId = (sectionSelectionRadios == "instructorLed") ? CourseViewModel.InstructorLedCourseVariantId : CourseViewModel.SelfPacedCourseVariantId
            };

            PromoCodeDto validPromoCode = null;
            // validate, consume, and apply promo code redemption 
            if (courseViewModel.PromoCodeRedemptionGuid.HasValue && courseViewModel.PromoCodeRedemptionGuid.Value != Guid.Empty)
            {
                validPromoCode = API.PromoCodeRedemptionValidation(courseViewModel.PromoCodeRedemptionGuid.Value.ToString(), Course.CourseGuid.Value.ToString(), this.subscriber.SubscriberGuid.Value.ToString());

                if (validPromoCode == null)
                    return View("EnrollmentFailure", courseViewModel);
                else
                {
                    enrollmentDto.PricePaid -= validPromoCode.Discount;
                    enrollmentDto.PromoCodeRedemptionGuid = validPromoCode.PromoCodeRedemptionGuid;
                }
            }

            // process payment if price is not zero
            if (enrollmentDto.PricePaid != 0)
            {

                BraintreePaymentDto BraintreePaymentDto = new BraintreePaymentDto
                {
                    PaymentAmount = enrollmentDto.PricePaid,
                    Nonce = Request.Form["payment_method_nonce"],
                    FirstName = courseViewModel.BillingFirstName,
                    LastName = courseViewModel.BillingLastName,
                    PhoneNumber = this.subscriber.PhoneNumber,
                    Email = this.subscriber.Email,
                    Address = courseViewModel.BillingAddress,
                    Region = courseViewModel.BillingState,
                    Locality = courseViewModel.BillingCity,
                    ZipCode = courseViewModel.BillingZipCode,
                    CountryCode = courseViewModel.BillingCountry,
                    MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("BraintreeMerchantAccountID")
                };
                BraintreeResponseDto brdto = API.SubmitBraintreePayment(BraintreePaymentDto);

                if (brdto.WasSuccessful)
                {
                    API.EnrollStudentAndObtainEnrollmentGUID(enrollmentDto);
                    return View("EnrollmentSuccess", courseViewModel);
                }
                else
                {
                    return View("EnrollmentFailure", courseViewModel);
                }
            }
            else
            {
                // course is free with promo code
                API.EnrollStudentAndObtainEnrollmentGUID(enrollmentDto);
                return View("EnrollmentSuccess", courseViewModel);
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
