using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/course-levels/")]
    public class CourseLevelsController : ControllerBase
    {
        private readonly ICourseLevelService _courseLevelService;

        public CourseLevelsController(ICourseLevelService courseLevelService)
        {
            _courseLevelService = courseLevelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var courseLevels = await _courseLevelService.GetCourseLevels(limit, offset, sort, order);
            return Ok(courseLevels);
        }

        [HttpGet]
        [Route("{courseLevel:guid}")]
        public async Task<IActionResult> GetCourseLevel(Guid courseLevel)
        {
            var result  = await _courseLevelService.GetCourseLevel(courseLevel);
            return Ok(result);
        }

        [HttpPut]
        [Route("{courseLevel:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCourseLevel(Guid courseLevel, [FromBody]  CourseLevelDto courseLevelDto)
        {
            await _courseLevelService.UpdateCourseLevel(courseLevel, courseLevelDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{courseLevel:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteCourseLevel(Guid courseLevel)
        {
            await _courseLevelService.DeleteCourseLevel(courseLevel);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateCourseLevel([FromBody] CourseLevelDto courseLevelDto)
        {
            await _courseLevelService.CreateCourseLevel(courseLevelDto);
            return StatusCode(201);
        }
       
    }
}
