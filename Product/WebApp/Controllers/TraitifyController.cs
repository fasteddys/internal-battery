using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using com.traitify.net.TraitifyLibrary;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using System.Threading.Tasks;
namespace UpDiddy.Controllers
{
    public class TraitifyController : BaseController
    {
        private IApi _api;
        private readonly IConfiguration _config;

        public TraitifyController(IApi api,
         IConfiguration config) : base(api)
        {
            //TODO get keys from config
            // string publicKey = _config["Traitify:PublicKey"];
            // string secretKey = _config["Traitify:SecretKey"];
            // string hostUrl = _config["Traitify:HostUrl"];

            _api = api;
            _config = config;

        }

        [HttpGet]
        [Route("[controller]")]
        public IActionResult Index()
        {
            
            TraitifyViewModel model = new TraitifyViewModel();
            return View(model);
        }


        [HttpPost]
        [Route("[controller]")]
        public async Task<IActionResult> Index(TraitifyViewModel model)
        {
            TraitifyDto dto = new TraitifyDto() {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email
            };
            var result = await _api.StartNewTraitifyAssessment(dto);
            ViewBag.assessmentId = result.AssessmentId;
            ViewBag.publicKey = result.PublicKey;
            ViewBag.host = result.Host;
            return View("Assessment");
        }

        
    }
}