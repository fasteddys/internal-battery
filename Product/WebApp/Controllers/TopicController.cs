using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using UpDiddy.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
  
    public class TopicController : Controller
    {

        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IConfiguration _configuration;

        public TopicController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<HomeController> localizer, IConfiguration configuration)
        {
            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("/Topic/{TopicSlug}")]
        public IActionResult Get(string TopicSlug)
        {
            ApiUpdiddy API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, _configuration);
            string TopicName = API.TopicBySlug(TopicSlug).Name;
            CourseViewModel CourseViewModel = new CourseViewModel(_configuration, API.Courses(TopicSlug), TopicName);

            return View("Details", CourseViewModel);
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            
            // Create Interface and use DI to inject 
            ApiUpdiddy API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, _configuration);
            TopicViewModel TopicViewModel = new TopicViewModel(_configuration, API.Topics());       
            return View(TopicViewModel);
        }
    }
}
