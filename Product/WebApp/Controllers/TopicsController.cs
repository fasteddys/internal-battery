using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using UpDiddy.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{

    [Route("[controller]")]
    public class TopicsController : BaseController
    {
        private readonly IConfiguration _configuration;       

        public TopicsController(IApi api, IConfiguration configuration)
             : base(api)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            // TODO: We don't have the Partners linked to the courses that they offer... 
            TopicsViewModel TopicsViewModel = new TopicsViewModel(_configuration)
            {
                Topics = await _Api.TopicsAsync()
            };

            return View("Index", TopicsViewModel);
        }

        [HttpGet("{TopicSlug}")]
        public async System.Threading.Tasks.Task<IActionResult> GetAsync(string TopicSlug)
        {         
            TopicDto Topic = await _Api.TopicBySlugAsync(TopicSlug);
            TopicViewModel TopicViewModel = new TopicViewModel(_configuration, await _Api.getCoursesByTopicSlugAsync(TopicSlug), Topic);
        
            return View("Details", TopicViewModel);
        }
        
    }
}
