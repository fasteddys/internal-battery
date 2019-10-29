using System;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using System.Threading.Tasks;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using ButterCMS.Models;
using UpDiddyLib.Helpers;
using UpDiddyLib.Dto.Marketing;
using System.Linq;
using UpDiddyLib.Dto.User;

namespace UpDiddy.Controllers
{
    public class TraitifyController : BaseController
    {
        private readonly IButterCMSService _butterService;
        public TraitifyController(IApi api,
         IButterCMSService butterService,
         IConfiguration config) : base(api, config)
        {
            _butterService = butterService;
        }

        [HttpGet]
        [Route("[controller]")]
        public async Task<IActionResult> Index()
        {
            TraitifyViewModel model = new TraitifyViewModel();
            var butterPage = await GetButterLandingPage();
            model = PopulateButterFields(model, butterPage);
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Guid? subscriberGuid = GetSubscriberGuid();
                if (subscriberGuid != null)
                {
                    SubscriberDto subscriber = await _Api.SubscriberAsync(subscriberGuid.Value, true);
                    model.SubscriberGuid = subscriberGuid;
                    model.FirstName = subscriber.FirstName = subscriber != null ? subscriber.FirstName : string.Empty;
                    model.LastName = subscriber.LastName = subscriber.LastName != null ? subscriber.LastName : string.Empty;
                    model.Email = subscriber.Email != null ? subscriber.Email : string.Empty;
                    model.IsAuthenticated = true;
                }
            }
            SetSEOTags(butterPage);
            return View(model);
        }

        [HttpPost]
        [Route("[controller]")]
        public async Task<IActionResult> Index(TraitifyViewModel model)
        {
            var butterPage = await GetButterLandingPage();
            model = PopulateButterFields(model, butterPage);
            if (!ModelState.IsValid && model.SubscriberGuid == null)
            {
                return View(model);
            }

            TraitifyDto dto = new TraitifyDto()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                SubscriberGuid = model.SubscriberGuid
            };

            var result = await _Api.StartNewTraitifyAssessment(dto);
            model.AssessmentId = result.AssessmentId;
            model.PublicKey = result.PublicKey;
            model.Host = result.Host;
            return View("Assessment", model);
        }

        [HttpPost]
        [Route("[controller]/createaccount")]
        public async Task<ActionResult> CreateAccount(TraitifyViewModel model)
        {
            bool modelHasAllFields = !string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.ReenterPassword);

            if (!modelHasAllFields)
            {
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Please enter all sign-up fields and try again."
                });
            }

            var emailModelState = ModelState.Where(x => x.Key == "Email").FirstOrDefault().Value;
            if (emailModelState.Errors.Count > 0)
            {
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = emailModelState.Errors.FirstOrDefault().ErrorMessage
                });
            }

            var passwordModelState = ModelState.Where(x => x.Key == "Password").FirstOrDefault().Value;
            if (passwordModelState.Errors.Count > 0)
            {
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = passwordModelState.Errors.FirstOrDefault().ErrorMessage
                });
            }


            try
            {
                TraitifyDto traitifyDto = await _Api.GetTraitifyByAssessmentId(model.AssessmentId);
                CreateUserDto createUserDto = new CreateUserDto()
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = traitifyDto.FirstName,
                    LastName = traitifyDto.LastName,
                    PartnerGuid = model.PartnerGuid,
                    ReferrerUrl = _configuration["Environment:BaseUrl"] + "traitify"
                };
                BasicResponseDto subscriberResponse = await _Api.CreateUserAsync(createUserDto);
                switch (subscriberResponse.StatusCode)
                {
                    case 200:
                        var subscriberGuid = Guid.Parse(subscriberResponse.Data.subscriberGuid.ToString());
                        _Api.AssociateSubscriberWithAssessment(traitifyDto.AssessmentId, subscriberGuid);
                        return Ok(new BasicResponseDto
                        {
                            StatusCode = subscriberResponse.StatusCode,
                            Description = "/session/signin"
                        });
                    default:
                        return StatusCode(500, subscriberResponse);
                }
            }
            catch (ApiException e)
            {
                // Generic server error to display gracefully to the user.
                return StatusCode(500, new BasicResponseDto
                {
                    StatusCode = 500,
                    Description = e.ResponseDto.Description
                });
            }
        }

        [HttpGet]
        [Route("[controller]/{assessmentId:length(36)}")]
        public async Task<IActionResult> GetByAssessmentId(string assessmentId)
        {
            TraitifyViewModel model = new TraitifyViewModel();
            var butterPage = await GetButterLandingPage();
            model = PopulateButterFields(model, butterPage);
            TraitifyDto dto = await _Api.GetTraitifyByAssessmentId(assessmentId);
            if (dto != null)
            {
                model.IsComplete = dto.IsComplete;
                model.AssessmentId = dto.AssessmentId;
                model.PublicKey = dto.PublicKey;
                model.Host = dto.Host;
                model.Email = dto.Email;
                model.IsRegistered = dto.IsRegistered;
            }
            else
            {
                return Redirect("/traitify");
            }
            return View("Assessment", model);
        }

        [HttpPost]
        [Route("[controller]/complete/{assessmentId:length(36)}")]
        public async Task<JsonResult> CompleteAssessment(string assessmentId)
        {
            var response = await _Api.CompleteAssessment(assessmentId);
            var IsAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            return Json(new { email = response.Email, IsAuthenticated = IsAuthenticated });
        }

        private async Task<PageResponse<TraitifyLandingPageViewModel>> GetButterLandingPage()
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<TraitifyLandingPageViewModel> landingPage = await _butterService.RetrievePageAsync<TraitifyLandingPageViewModel>("/traitify", QueryParams);
            return landingPage;
        }

        private TraitifyViewModel PopulateButterFields(TraitifyViewModel model, PageResponse<TraitifyLandingPageViewModel> landingPage)
        {
            model.HeroImage = landingPage.Data.Fields.HeroImage;
            model.HeroHeader = landingPage.Data.Fields.HeroHeader;
            model.HeroDescription = landingPage.Data.Fields.HeroDescription;
            model.ModalHeader = landingPage.Data.Fields.ModalHeader;
            model.ModalText = landingPage.Data.Fields.ModalText;
            model.FormHeader = landingPage.Data.Fields.FormHeader;
            model.FormText = landingPage.Data.Fields.FormText;
            model.FormButtonText = landingPage.Data.Fields.FormSubmitButtonText;
            model.ExistingUserButtonText = landingPage.Data.Fields.ExistingUserSubmitButtonText;
            model.SignupFormImageBanner = landingPage.Data.Fields.SignupFormImageBanner;
            model.SignupFormHeader = landingPage.Data.Fields.SignupFormHeader;
            model.SignupFormSubmitButtonText = landingPage.Data.Fields.SignupFormSubmitButtonText;
            model.SignupFormText = landingPage.Data.Fields.SignupFormText;
            model.SignupHeroContent = landingPage.Data.Fields.SignupHeroContent;
            model.SignupHeroTitle = landingPage.Data.Fields.SignupHeroTitle;
            model.ResultFooterText = landingPage.Data.Fields.ResultFooterText;
            Guid partnerGuid;
            if (Guid.TryParse(landingPage.Data.Fields.Partner.Guid, out partnerGuid))
                model.PartnerGuid = partnerGuid;
            return model;
        }

        private void SetSEOTags(PageResponse<TraitifyLandingPageViewModel> landingPage)
        {
            ViewData[Constants.Seo.TITLE] = landingPage.Data.Fields.Title;
            ViewData[Constants.Seo.META_DESCRIPTION] = landingPage.Data.Fields.MetaDescription;
            ViewData[Constants.Seo.META_KEYWORDS] = landingPage.Data.Fields.MetaKeywords;
            ViewData[Constants.Seo.OG_TITLE] = landingPage.Data.Fields.OpenGraphTitle;
            ViewData[Constants.Seo.OG_DESCRIPTION] = landingPage.Data.Fields.OpenGraphDescription;
            ViewData[Constants.Seo.OG_IMAGE] = landingPage.Data.Fields.OpenGraphImage;
        }
    }
}