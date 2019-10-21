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
            if (!ModelState.IsValid && model.SubscriberGuid == null)
            {
                model = PopulateButterFields(model, butterPage);
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
            model.ModalHeader = butterPage.Data.Fields.ModalHeader;
            model.ModalText = butterPage.Data.Fields.ModalText;
            model.AssessmentId = result.AssessmentId;
            model.PublicKey = result.PublicKey;
            model.Host = result.Host;
            return View("Assessment", model);
        }

        [HttpPost]
        [Route("[controller]/createaccount")]
        public async Task CreateAccount(TraitifyViewModel model)
        {
            SignUpDto signUpDto = new SignUpDto
            {
                firstName = model.FirstName,
                lastName = model.LastName,
                email = model.Email,
                password = model.Password,
                traitifyAssessmentId = model.AssessmentId
            };
            await _Api.ExpressUpdateSubscriberContactAsync(signUpDto);
        }

        [HttpGet]
        [Route("[controller]/{assessmentId:length(36)}")]
        public async Task<IActionResult> GetByAssessmentId(string assessmentId)
        {
            TraitifyViewModel model = new TraitifyViewModel();
            var butterPage = await GetButterLandingPage();
            model.ModalHeader = butterPage.Data.Fields.ModalHeader;
            model.ModalText = butterPage.Data.Fields.ModalText;
            TraitifyDto dto = await _Api.GetTraitifyByAssessmentId(assessmentId);
            if (dto != null)
            {
                model.AssessmentId = dto.AssessmentId;
                model.PublicKey = dto.PublicKey;
                model.Host = dto.Host;
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
            return Json(new { success = response, IsAuthenticated = IsAuthenticated });

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