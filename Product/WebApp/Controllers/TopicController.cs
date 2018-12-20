using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using UpDiddy.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
  
    public class TopicController : BaseController
    {
        private readonly IConfiguration _configuration;       

        public TopicController(IApi api, IConfiguration configuration)
             : base(api)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("/Topic/{TopicSlug}")]
        public IActionResult Get(string TopicSlug)
        {         
            TopicDto Topic = _Api.TopicBySlug(TopicSlug);
            TopicViewModel TopicViewModel = new TopicViewModel(_configuration, _Api.getCoursesByTopicSlug(TopicSlug), Topic);
        
            return View("Details", TopicViewModel);
        }

        // GET: /<controller>/
        public IActionResult Index()
        {               
            return View();
        }
    }
}
