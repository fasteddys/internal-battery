using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/education-levels/")]
    public class EducationLevelsController : ControllerBase
    {
        private readonly IEducationLevelService _educationLevelService;

        public EducationLevelsController(IEducationLevelService educationLevelService)
        {
            _educationLevelService = educationLevelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEducationLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var EducationLevels = await _educationLevelService.GetEducationLevels(limit, offset, sort, order);
            return Ok(EducationLevels);
        }

        [HttpGet]
        [Route("{educationLevel:guid}")]
        public async Task<IActionResult> GetEducationLevel(Guid educationLevel)
        {
            var result  = await _educationLevelService.GetEducationLevel(educationLevel);
            return Ok(result);
        }

        [HttpPut]
        [Route("{educationLevel:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateEducationLevel(Guid educationLevel, [FromBody]  EducationLevelDto educationLevelDto)
        {
            await _educationLevelService.UpdateEducationLevel(educationLevel, educationLevelDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{educationLevel:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteEducationLevel(Guid educationLevel)
        {
            await _educationLevelService.DeleteEducationLevel(educationLevel);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateEducationLevel([FromBody] EducationLevelDto educationLevelDto)
        {
            await _educationLevelService.CreateEducationLevel(educationLevelDto);
            return StatusCode(201);
        }
       
    }
}
