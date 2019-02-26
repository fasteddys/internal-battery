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
        public async System.Threading.Tasks.Task<IActionResult> GetAsync(string TopicSlug)
        {         
            TopicDto Topic = await _Api.TopicBySlugAsync(TopicSlug);
            TopicViewModel TopicViewModel = new TopicViewModel(_configuration, await _Api.getCoursesByTopicSlugAsync(TopicSlug), Topic);
        
            return View("Details", TopicViewModel);
        }

        // GET: /<controller>/
        public IActionResult Index()
        {               
            return View();
        }
    }
}
