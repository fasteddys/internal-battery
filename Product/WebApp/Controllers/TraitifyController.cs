using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using com.traitify.net.TraitifyLibrary;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using UpDiddy.ViewModels;

namespace UpDiddy.Controllers
{
    public class TraitifyController : BaseController
    {
        private IApi _api;
        private readonly IConfiguration _config;

        private readonly Traitify _traitify;
        public TraitifyController(IApi api,
         IConfiguration config) : base(api)
        {
            //TODO get keys from config
            // string publicKey = _config["Traitify:PublicKey"];
            // string secretKey = _config["Traitify:SecretKey"];
            // string hostUrl = _config["Traitify:HostUrl"];

            _api = api;
            _config = config;
            _traitify = new Traitify("https://api.traitify.com", "3d731f347b674c7da1c55b25aa172314", "cb156aed701d491f9a8114896b5d9a1f", "v1");
        }

        [HttpGet]
        [Route("[controller]")]
        public IActionResult Index()
        {
            
            var assesment = _traitify.CreateAssesment("career-deck");
            var slides = _traitify.GetSlides(assesment.id);
            TraitifyViewModel model = new TraitifyViewModel()
            {
                AssesmentId = assesment.id,
                Slides = slides
            };

            return View(model);
        }

        [HttpPost]
        [Route("[controller]/submit")]
        public IActionResult Submit([FromBody] TraitifyViewModel model)
        {
            _traitify.SetSlideBulkUpdate(model.AssesmentId, model.Slides);
            var result = new TraitifyResultViewModel()
            {
                AssessmentPersonalityTraits = _traitify.GetPersonalityTraits(model.AssesmentId),
                AssessmentPersonalityTypes = _traitify.GetPersonalityTypes(model.AssesmentId)
            };
            return View("result", model);
        }

        [HttpGet]
        [Route("[controller]/iantest")]
        public IActionResult IanTest()
        {
            var result = new TraitifyResultViewModel()
            {
                AssessmentPersonalityTraits = _traitify.GetPersonalityTraits("36215e43-236f-4ba1-9cbc-d1b8566b3024"),
                AssessmentPersonalityTypes = _traitify.GetPersonalityTypes("36215e43-236f-4ba1-9cbc-d1b8566b3024")
            };
            return View();
        }
    }
}