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
using UpDiddy.Authentication;
using System.Threading.Tasks;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class CourseController : BaseController
    {
        private readonly IConfiguration _configuration;
        private IBraintreeConfiguration braintreeConfiguration;
        private IButterCMSService _butterService;
        private static readonly TransactionStatus[] transactionSuccessStatuses = {
                TransactionStatus.AUTHORIZED,
                TransactionStatus.AUTHORIZING,
                TransactionStatus.SETTLED,
                TransactionStatus.SETTLING,
                TransactionStatus.SETTLEMENT_CONFIRMED,
                TransactionStatus.SETTLEMENT_PENDING,
                TransactionStatus.SUBMITTED_FOR_SETTLEMENT
            };

        public CourseController(IApi api, IConfiguration configuration, IButterCMSService butterCMSService) : base(api)
        {
            _configuration = configuration;
            braintreeConfiguration = new BraintreeConfiguration(_configuration);
            _butterService = butterCMSService;

        }        

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("/Course/PromoCodeValidation/{code}/{courseVariantGuid}")]
        public async Task<IActionResult> PromoCodeValidation(string code, string courseVariantGuid)
        {
            PromoCodeDto promoCodeDto = await _Api.PromoCodeValidationAsync(code, courseVariantGuid);
            return new ObjectResult(promoCodeDto);
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpGet]
        [Route("/Course/Checkout/{CourseSlug}", Name = "CourseCheckout")]
        public async Task<IActionResult> GetAsync(string CourseSlug)
        {
            var gateway = braintreeConfiguration.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;

            try
            {
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
                    RebateOffer = this.subscriber.EligibleCampaigns.SelectMany(ec => ec.CampaignCourseVariant).Where(ccv => ccv.CourseVariant.CourseVariantGuid == dto.CourseVariantGuid).FirstOrDefault()?.RebateType?.Description,
                    RebateTerms = this.subscriber.EligibleCampaigns.SelectMany(ec => ec.CampaignCourseVariant).Where(ccv => ccv.CourseVariant.CourseVariantGuid == dto.CourseVariantGuid).FirstOrDefault()?.RebateType?.Terms
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
            catch(ApiException ex)
            {
                return StatusCode((int) ex.StatusCode);
            }
        }
        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost]
        public async Task<IActionResult> Checkout(CourseViewModel courseViewModel)
        {
            DateTime currentDate = DateTime.UtcNow;
       

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

            // retrieve the campaign course variant associated with this subscriber (if one exists)
            var campaignCourseVariant = this.subscriber.EligibleCampaigns
                .SelectMany(ec => ec.CampaignCourseVariant)
                .Where(ccv => ccv.CourseVariant.CourseVariantGuid == selectedCourseVariant.CourseVariantGuid)
                .FirstOrDefault();

            if (campaignCourseVariant != null)
            {
                // this is kind of kludgey... after the EligibleCampaigns comes back over the wire from the API, the Campaign property
                // is lost. this isn't happening within the API. rather than continue to troubleshoot the issue, i'm just going to set 
                // the Campaign property of the CampaignCourseVariant using the object i already have (if a match exists on selectedCourseVariant)
                campaignCourseVariant.Campaign = this.subscriber.EligibleCampaigns
                    .Where(ec => ec.CampaignCourseVariant
                        .Where(ccv => ccv.CourseVariant.CourseVariantGuid == selectedCourseVariant.CourseVariantGuid).Any())
                    .FirstOrDefault();

                // doing this because of a circular reference error during JSON serialization when trying to call UpDiddyApi. i tried
                // adding the same code that we currently have in the api's startup to ignore circular references to the webapp's startup
                // but still received the same exception. workaround is to null this out to prevent the circular reference.
                campaignCourseVariant.Campaign.CampaignCourseVariant = null;
            }

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
                    await _Api.UpdateProfileInformationAsync(new SubscriberDto()
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
                    CampaignCourseVariant = campaignCourseVariant
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

                await _Api.EnrollStudentAndObtainEnrollmentGUIDAsync(enrollmentFlowDto);
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

                // todo: course skills aren't present on page if model state is invalid
                return View(courseViewModel);
            }
        }

        [Authorize]
        [HttpGet("[controller]/ClinicalResearchCoordinator")]
        public IActionResult ClinicalResearchCoordinator()
        {
            return View("ClinicalResearchCoordinator");
        }

        [Authorize]
        [HttpGet("[controller]/itprotv-comptia-it-fundamentals")]
        public IActionResult ITProTVCourse()
        {
            return View("ITProTVCourse");
        }

        [Authorize]
        [HttpGet("[controller]/{slug}")]
        public async Task<IActionResult> CmsCourse(string slug){
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<CmsCourseViewModel> cmsCourse = await _butterService.RetrievePageAsync<CmsCourseViewModel>("course_" + slug, slug, QueryParams);
            if(cmsCourse == null)
                return NotFound();

            CmsCourseViewModel course = new CmsCourseViewModel{
                Band1ImagePath = cmsCourse.Data.Fields.Band1ImagePath,
                Band2Text = cmsCourse.Data.Fields.Band2Text,
                Band2Title = cmsCourse.Data.Fields.Band2Title,
                Band3LeftText = cmsCourse.Data.Fields.Band3LeftText,
                Band3RightText = cmsCourse.Data.Fields.Band3RightText,
                Band4ButtonUrl = cmsCourse.Data.Fields.Band4ButtonUrl,
                Band4Text = cmsCourse.Data.Fields.Band4Text,
                Band4Title = cmsCourse.Data.Fields.Band4Title,
                Band4ButtonText = cmsCourse.Data.Fields.Band4ButtonText
            };
            
            return View(course);
        }
    }
}
