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
namespace UpDiddy.Controllers
{
    public class TraitifyController : BaseController
    {
        private IApi _api;
        private readonly IConfiguration _config;
        private readonly IButterCMSService _butterService;
        public TraitifyController(IApi api,
         IButterCMSService butterService,
         IConfiguration config) : base(api)
        {
            _api = api;
            _config = config;
            _butterService = butterService;
        }

        [HttpGet]
        [Route("[controller]")]
        public async Task<IActionResult> Index()
        {
            TraitifyViewModel model = new TraitifyViewModel();
            var butterPage = await GetButterLandingPage();
            model = PopulateButterFields(model,butterPage);
            model.SubscriberGuid = HttpContext.User.Identity.IsAuthenticated ? GetSubscriberGuid() : (Guid?)null;
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

            var result = await _api.StartNewTraitifyAssessment(dto);
            model.ModalHeader = butterPage.Data.Fields.ModalHeader;
            model.ModalText = butterPage.Data.Fields.ModalText;
            model.AssessmentId = result.AssessmentId;
            model.PublicKey = result.PublicKey;
            model.Host = result.Host;
            return View("Assessment", model);
        }

        [HttpGet]
        [Route("[controller]/{assessmentId?}")]
        public async Task<IActionResult> GetByAssessmentId(string assessmentId)
        {
            TraitifyViewModel model = new TraitifyViewModel();
            var butterPage = await GetButterLandingPage();
            model.ModalHeader = butterPage.Data.Fields.ModalHeader;
            model.ModalText = butterPage.Data.Fields.ModalText;
            TraitifyDto dto = await _api.GetTraitifyByAssessmentId(assessmentId);
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

        [HttpGet]
        [Route("[controller]/complete/{assessmentId?}")]
        public async Task<JsonResult> CompleteAssessment(string assessmentId)
        {
            var result = await _api.CompleteAssessment(assessmentId);
            return Json(result);
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