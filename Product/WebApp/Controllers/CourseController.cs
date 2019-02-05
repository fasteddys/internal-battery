using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers.Braintree;
using UpDiddyLib.Helpers;
using Braintree;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpDiddy.Api;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class CourseController : BaseController
    {
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

        public CourseController(IApi api, IConfiguration configuration) : base(api)
        {
            _configuration = configuration;
            braintreeConfiguration = new BraintreeConfiguration(_configuration);

        }

        public IActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("/Course/PromoCodeValidation/{code}/{courseVariantGuid}")]
        public async System.Threading.Tasks.Task<IActionResult> PromoCodeValidation(string code, string courseVariantGuid)
        {
            PromoCodeDto promoCodeDto = await _Api.PromoCodeValidationAsync(code, courseVariantGuid);
            return new ObjectResult(promoCodeDto);
        }


        [Authorize]
        [HttpGet]
        [Route("/Course/Checkout/{CourseSlug}")]
        public async System.Threading.Tasks.Task<IActionResult> Get(string CourseSlug)
        {
            await GetSubscriberAsync(false);

            var gateway = braintreeConfiguration.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            var course = await _Api.CourseAsync(CourseSlug);
            
            var courseVariantViewModels = course.CourseVariants.Select(dto => new CourseVariantViewModel()
            {
                CourseVariantGuid = dto.CourseVariantGuid,
                CourseVariantType = dto.CourseVariantType.Name,
                Price = dto.Price,
                StartDates = dto.StartDateUTCs?.Select(i => new SelectListItem()
                {
                    Text = i.ToShortDateString(),
                    Value = i.ToString()
                }),
                IsEligibleCampaignOffer = this.subscriber.EligibleCampaigns.SelectMany(ec => ec.CampaignCourseVariant).Where(ccv => ccv.CourseVariant.CourseVariantGuid == dto.CourseVariantGuid).Any(),
                RebateOffer = this.subscriber.EligibleCampaigns.SelectMany(ec => ec.CampaignCourseVariant).Where(ccv => ccv.CourseVariant.CourseVariantGuid == dto.CourseVariantGuid).FirstOrDefault()?.RebateType?.Description
            });

            var countries = await _Api.GetCountriesAsync();
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
                FormattedPhone = this.subscriber.PhoneNumber,
                SubscriberGuid = this.subscriber.SubscriberGuid.Value,
                TermsOfServiceContent = course.TermsOfServiceContent,
                TermsOfServiceDocumentId = course.TermsOfServiceDocumentId,
                Skills = course.Skills,
                Countries = countries.Select(c => new SelectListItem()
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
        public async System.Threading.Tasks.Task<IActionResult> Checkout(CourseViewModel courseViewModel)
        {
            DateTime currentDate = DateTime.UtcNow;
            await GetSubscriberAsync(false);


            // get the course variant based on the Guid selected by the user
            // normally we would want everything to come back via the view model, but we don't want to trust this because it contains price information
            CourseVariantViewModel selectedCourseVariant = null;
            if (courseViewModel.SelectedCourseVariant.HasValue)
            {
                CourseVariantDto courseVariantDto = null;
                courseVariantDto = await _Api.GetCourseVariantAsync(courseViewModel.SelectedCourseVariant.Value);
                selectedCourseVariant = new CourseVariantViewModel()
                {
                    CourseVariantGuid = courseVariantDto.CourseVariantGuid,
                    CourseVariantType = courseVariantDto.CourseVariantType.Name,
                    Price = courseVariantDto.Price,
                    SelectedStartDate = courseViewModel.SelectedStartDate
                };
            }
            courseViewModel.CourseVariant = selectedCourseVariant;

            // retrieve the campaign associated with the selected course variant for this subscriber (if one exists)
            var campaign = this.subscriber.EligibleCampaigns
                .Where(ec => ec.CampaignCourseVariant.Exists(ccv => ccv.CourseVariant.CourseVariantGuid == selectedCourseVariant.CourseVariantGuid))
                .FirstOrDefault();

            // validate, consume, and apply promo code redemption. consider moving this to CourseViewModel.Validate (using IValidatableObject)
            PromoCodeDto validPromoCode = null;
            decimal adjustedPrice = courseViewModel.CourseVariant.Price;
            if (courseViewModel.PromoCodeRedemptionGuid.HasValue && courseViewModel.PromoCodeRedemptionGuid.Value != Guid.Empty)
            {
                validPromoCode = await _Api.PromoCodeRedemptionValidationAsync(courseViewModel.PromoCodeRedemptionGuid.Value.ToString(), courseViewModel.SelectedCourseVariant.ToString());

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
            if (selectedCourseVariant != null)
            {
                switch (selectedCourseVariant.CourseVariantType)
                {
                    case "Instructor-Led":
                        enrollmentStatusId = (int)EnrollmentStatus.FutureRegisterStudentRequested;
                        if (selectedCourseVariant.SelectedStartDate.HasValue)
                            sectionStartTimestamp = Utils.ToUnixTimeInMilliseconds(selectedCourseVariant.SelectedStartDate.Value);
                        else
                            ModelState.AddModelError("SelectedCourseVariant", "A start date must be selected for instructor-led courses.");
                        break;
                    case "Self-Paced":
                        enrollmentStatusId = (int)EnrollmentStatus.RegisterStudentRequested;
                        break;
                    default:
                        ModelState.AddModelError("SelectedCourseVariant", "A course section must be selected.");
                        break;
                }
            }

            if (ModelState.IsValid)
            {
                // update the subscriber's first name, last name, and phone number if they do not exist (or if the values are different than what is stored currently). 
                // we must do this here because first name, last name, and phone number are required for the enrollment process for Woz. 
                // todo: refactor this; consider creating a partial view that is shared between the profile page, checkout, and anywhere else where we need to work with the subscriber
                if (this.subscriber.FirstName == null || !this.subscriber.FirstName.Equals(courseViewModel.SubscriberFirstName)
                    || this.subscriber.LastName == null || !this.subscriber.LastName.Equals(courseViewModel.SubscriberLastName)
                    || this.subscriber.PhoneNumber == null || !this.subscriber.PhoneNumber.Equals(courseViewModel.SubscriberPhoneNumber))
                {
                    _Api.UpdateProfileInformation(new SubscriberDto()
                    {
                        SubscriberGuid = courseViewModel.SubscriberGuid,
                        FirstName = courseViewModel.SubscriberFirstName,
                        LastName = courseViewModel.SubscriberLastName,
                        PhoneNumber = courseViewModel.SubscriberPhoneNumber
                    });
                }

                // create the enrollment object
                EnrollmentDto enrollmentDto = new EnrollmentDto
                {
                    CreateDate = currentDate,
                    ModifyDate = currentDate,
                    DateEnrolled = currentDate,
                    CreateGuid = Guid.Empty,
                    ModifyGuid = Guid.Empty,
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
                    PromoCodeRedemptionGuid = courseViewModel.PromoCodeRedemptionGuid,
                    Campaign = campaign
                };

                BraintreePaymentDto BraintreePaymentDto = new BraintreePaymentDto
                {
                    PaymentAmount = enrollmentDto.PricePaid,
                    Nonce = Request.Form["payment_method_nonce"],
                    FirstName = courseViewModel.BillingFirstName,
                    LastName = courseViewModel.BillingLastName,
                    PhoneNumber = this.subscriber.PhoneNumber,
                    Email = this.subscriber.Email,
                    Address = courseViewModel.BillingAddress,
                    Locality = courseViewModel.BillingCity,
                    ZipCode = courseViewModel.BillingZipCode,
                    StateGuid = courseViewModel.SelectedState,
                    CountryGuid = courseViewModel.SelectedCountry,
                    MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("Braintree:MerchantAccountID")
                };

                EnrollmentFlowDto enrollmentFlowDto = new EnrollmentFlowDto
                {
                    EnrollmentDto = enrollmentDto,
                    BraintreePaymentDto = BraintreePaymentDto,
                    SubscriberDto = this.subscriber
                };

                _Api.EnrollStudentAndObtainEnrollmentGUID(enrollmentFlowDto);
                return View("EnrollmentSuccess", courseViewModel);
            }
            else
            {
                // model validation failed; return the view with a validation summary
                var gateway = braintreeConfiguration.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;

                var course = await _Api.CourseAsync(courseViewModel.Slug);
                // need to repopulate course variants since they were not bound in the postback
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
                courseViewModel.CourseVariants = courseVariantViewModels;

                var countries = await _Api.GetCountriesAsync();
                // need to repopulate countries since they were not bound in the postback
                courseViewModel.Countries = countries.Select(c => new SelectListItem()
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
