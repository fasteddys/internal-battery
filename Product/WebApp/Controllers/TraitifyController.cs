using System;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using com.traitify.net.TraitifyLibrary;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using System.Threading.Tasks;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using ButterCMS.Models;
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
            model = await MapButterData(model);
            model.SubscriberGuid = HttpContext.User.Identity.IsAuthenticated ? GetSubscriberGuid() : (Guid?)null;
            return View(model);
        }

        [HttpPost]
        [Route("[controller]")]
        public async Task<IActionResult> Index(TraitifyViewModel model)
        {
            if(!ModelState.IsValid && model.SubscriberGuid == null)
            {
                 model = await  MapButterData(model);
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

        private async Task<TraitifyViewModel> MapButterData(TraitifyViewModel model)
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<TraitifyLandingPageViewModel> landingPage = await  _butterService.RetrievePageAsync<TraitifyLandingPageViewModel>("/traitify", QueryParams);
            model.HeroImage = landingPage.Data.Fields.HeroImage;
            model.HeroHeader = landingPage.Data.Fields.HeroHeader;
            model.HeroDescription = landingPage.Data.Fields.HeroDescription;
            model.ModalHeader = landingPage.Data.Fields.ModalHeader;
            model.ModalText = landingPage.Data.Fields.ModalText;
            model.FormHeader = landingPage.Data.Fields.FormHeader;
            model.FormText = landingPage.Data.Fields.FormText;
            model.FormButtonText = landingPage.Data.Fields.FormSubmitButtonText;
            return model;
        }
    }
}