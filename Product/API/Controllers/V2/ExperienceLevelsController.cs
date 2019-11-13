using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/experience-levels/")]
    public class ExperienceLevelsController : ControllerBase
    {
        private readonly IExperienceLevelService _experienceLevelService;

        public ExperienceLevelsController(IExperienceLevelService experienceLevelService)
        {
            _experienceLevelService = experienceLevelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetExperienceLevels()
        {
            var experienceLevels = await _experienceLevelService.GetExperienceLevels();
            return Ok(experienceLevels);
        }

        [HttpGet]
        [Route("{experienceLevel}")]
        public async Task<IActionResult> GetExperienceLevel(Guid experienceLevel)
        {
            var result  = await _experienceLevelService.GetExperienceLevel(experienceLevel);
            return Ok(result);
        }

        [HttpPut]
        [Route("{experienceLevel}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateExperienceLevel(Guid experienceLevel, [FromBody]  ExperienceLevelDto experienceLevelDto)
        {
            await _experienceLevelService.UpdateExperienceLevel(experienceLevel, experienceLevelDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{experienceLevel}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteExperienceLevel(Guid experienceLevel)
        {
            await _experienceLevelService.DeleteExperienceLevel(experienceLevel);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateExperienceLevel([FromBody] ExperienceLevelDto experienceLevelDto)
        {
            await _experienceLevelService.CreateExperienceLevel(experienceLevelDto);
            return StatusCode(201);
        }
       
    }
}
