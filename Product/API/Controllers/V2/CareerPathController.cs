using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
    public class CareerPathController : ControllerBase
    {
        private readonly ITopicService _topicService;
        public CareerPathController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCareerPaths()
        {            
            var topics = await _topicService.GetTopics();
            return Ok(topics);
        }

        [HttpGet]
        [Route("{topicGuid:guid}/courses")]
        public async Task<IActionResult> GetCareerPathCourses(Guid topicGuid)
        {
            var courses = await _topicService.GetTopicCourses(topicGuid);
            return Ok(courses);
        }

        [HttpGet]
        [Route("{topicGuid:guid}/skills")]
        public async Task<IActionResult> GetCareerPathSkills(Guid topicGuid)
        {
            var skills = await _topicService.GetTopicSkills(topicGuid);
            return Ok(skills);
        }
    }
}