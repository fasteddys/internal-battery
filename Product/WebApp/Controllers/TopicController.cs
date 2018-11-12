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
using System.Net.Http;
using Polly.Registry;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
  
    public class TopicController : BaseController
    {

        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IConfiguration _configuration;       
        private IHttpClientFactory _HttpClientFactory = null;


        public TopicController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<HomeController> localizer, IConfiguration configuration, IHttpClientFactory httpClientFactory )
             : base(azureAdB2COptions.Value, configuration, httpClientFactory)
        {
            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
            _HttpClientFactory = httpClientFactory;              
        }

        [HttpGet]
        [Route("/Topic/{TopicSlug}")]
        public IActionResult Get(string TopicSlug)
        {         
            TopicDto Topic = API.TopicBySlug(TopicSlug);
            TopicViewModel TopicViewModel = new TopicViewModel(_configuration, API.getCousesByTopicSlug(TopicSlug), Topic);
        
            return View("Details", TopicViewModel);
        }

        // GET: /<controller>/
        public IActionResult Index()
        {               
            return View();
        }
    }
}
