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
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            TraitifyViewModel model = new TraitifyViewModel();
            PageResponse<TraitifyLandingPageViewModel> landingPage = await  _butterService.RetrievePageAsync<TraitifyLandingPageViewModel>("/traitify", QueryParams);
            model.HeroImage = landingPage.Data.Fields.HeroImage;
            model.HeroHeader = landingPage.Data.Fields.HeroHeader;
            model.HeroDescription = landingPage.Data.Fields.HeroDescription;
            return View(model);
        }

        [HttpPost]
        [Route("[controller]/{assessmentId?}")]
        public async Task<IActionResult> Index(TraitifyViewModel model)
        {
            if(!ModelState.IsValid)
            {
                 return View(model); 
            }
            
            TraitifyDto dto = new TraitifyDto()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email
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
                ViewBag.Invalid = true;
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
    }
}