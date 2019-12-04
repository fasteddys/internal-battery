using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
    public class CareerPathController : ControllerBase
    {
        private readonly ICareerPathService _careerPathService;

        public CareerPathController(ICareerPathService careerPathService)
        {
            _careerPathService = careerPathService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCareerPaths(int limit, int offset, string sort, string order)
        {
            var careerPaths = await _careerPathService.GetCareerPaths(limit, offset, sort, order);
            return Ok(careerPaths);
        }

        [HttpGet]
        [Route("{careerPathGuid:guid}/courses")]
        public async Task<IActionResult> GetCareerPathCourses(Guid careerPathGuid)
        {
            var result = await _careerPathService.GetCareerPathCourses(careerPathGuid);
            return Ok(result);
        }

        [HttpGet]
        [Route("{careerPathGuid:guid}/skills")]
        public async Task<IActionResult> GetCareerPathSkills(Guid careerPathGuid)
        {
            var result = await _careerPathService.GetCareerPathSkills(careerPathGuid);
            return Ok(result);
        }
    }
}