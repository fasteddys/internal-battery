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
                Slug = course.Slug,
                CourseGuid = course.CourseGuid.Value,
                CourseVariants = courseVariantViewModels,
                SubscriberFirstName = this.subscriber.FirstName,
                SubscriberLastName = this.subscriber.LastName,
                SubscriberGuid = this.subscriber.SubscriberGuid.Value,
                TermsOfServiceContent = course.TermsOfServiceContent,
                TermsOfServiceDocumentId = course.TermsOfServiceDocumentId,
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
            DateTime currentDate = DateTime.UtcNow;
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

            if (ModelState.IsValid)
            {
                // get the course variant based on the Guid selected, do not trust Price and Type from user submission
                CourseVariantViewModel selectedCourseVariant = null;
                if (courseViewModel.SelectedCourseVariant.HasValue)
                {
                    CourseVariantDto courseVariantDto = null;
                    courseVariantDto = API.GetCourseVariant(courseViewModel.SelectedCourseVariant.Value);
                    selectedCourseVariant = new CourseVariantViewModel()
                    {
                        CourseVariantGuid = courseVariantDto.CourseVariantGuid,
                        CourseVariantType = courseVariantDto.CourseVariantType.Name,
                        Price = courseVariantDto.Price,
                        SelectedStartDate = courseViewModel.SelectedStartDate
                    };
                }
                courseViewModel.CourseVariant = selectedCourseVariant;
                
                // validate, consume, and apply promo code redemption. consider moving this to CourseViewModel.Validate (using IValidatableObject)
                PromoCodeDto validPromoCode = null;
                decimal adjustedPrice = courseViewModel.CourseVariant.Price;
                if (courseViewModel.PromoCodeRedemptionGuid.HasValue && courseViewModel.PromoCodeRedemptionGuid.Value != Guid.Empty)
                {
                    validPromoCode = API.PromoCodeRedemptionValidation(courseViewModel.PromoCodeRedemptionGuid.Value.ToString(), courseViewModel.SelectedCourseVariant.ToString(), this.subscriber.SubscriberGuid.Value.ToString());

                    if (validPromoCode == null)
                        ModelState.AddModelError("PromoCodeRedemptionGuid", "The promo code selected is not valid for this course section.");
                    else
                    {
                        adjustedPrice -= validPromoCode.Discount;
                        courseViewModel.PromoCodeRedemptionGuid = validPromoCode.PromoCodeRedemptionGuid;
                    }
                }

                // use course variant type to infer enrollment status and start date
                int enrollmentStatusId = 0;
                long? sectionStartTimestamp = null;
                switch (selectedCourseVariant.CourseVariantType)
                {
                    case "Instructor-Led":
                        enrollmentStatusId = (int)EnrollmentStatus.FutureRegisterStudentRequested;
                        if (selectedCourseVariant.SelectedStartDate.HasValue)
                            sectionStartTimestamp = Utils.ToWozTime(selectedCourseVariant.SelectedStartDate.Value);
                        break;
                    case "Self-Paced":
                        enrollmentStatusId = (int)EnrollmentStatus.RegisterStudentRequested;
                        break;
                    default:
                        throw new ApplicationException("Unrecognized course variant; cannot determine enrollment status!");
                }

                // create the enrollment object
                EnrollmentDto enrollmentDto = new EnrollmentDto
                {
                    CreateDate = currentDate,
                    ModifyDate = currentDate,
                    DateEnrolled = currentDate,
                    CreateGuid = Guid.NewGuid(),
                    ModifyGuid = Guid.NewGuid(),
                    CourseGuid = courseViewModel.CourseGuid,
                    EnrollmentGuid = Guid.NewGuid(),
                    SubscriberId = this.subscriber.SubscriberId,
                    PricePaid = adjustedPrice,
                    PercentComplete = 0,
                    IsRetake = 0, //TODO Make this check DB for existing entry=
                    EnrollmentStatusId = enrollmentStatusId,
                    SectionStartTimestamp = sectionStartTimestamp,
                    TermsOfServiceFlag = courseViewModel.TermsOfServiceDocumentId,
                    Subscriber = this.subscriber,
                    CourseVariantGuid = courseViewModel.CourseVariant.CourseVariantGuid,
                    PromoCodeRedemptionGuid = courseViewModel.PromoCodeRedemptionGuid
                };

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
                        Region = string.Empty, // fix this
                        Locality = courseViewModel.BillingCity,
                        ZipCode = courseViewModel.BillingZipCode,
                        CountryCode = string.Empty, // fix this
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
            }
            else
            {
                // model validation failed; return the view with a validation summary
                var gateway = braintreeConfiguration.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;

                // need to repopulate course variants since they were not bound in the postback
                var courseVariantViewModels = API.Course(courseViewModel.Slug).CourseVariants.Select(dto => new CourseVariantViewModel()
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
                courseViewModel.CourseVariants = courseVariantViewModels;

                // need to repopulate countries since they were not bound in the postback
                courseViewModel.Countries = API.GetCountries().Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString()
                });

                courseViewModel.States = new List<StateViewModel>().Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.StateGuid.ToString()
                });
                
                return View(courseViewModel);
            }
        }
    }
}
