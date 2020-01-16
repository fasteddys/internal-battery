using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/educational-degree-types/")]
    public class EducationalDegreeTypesController : ControllerBase
    {
        private readonly IEducationalDegreeTypeService _educationalDegreeTypeService;

        public EducationalDegreeTypesController(IEducationalDegreeTypeService educationalDegreeTypeService)
        {
            _educationalDegreeTypeService = educationalDegreeTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEducationalDegreeTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var educationalDegreeTypes = await _educationalDegreeTypeService.GetEducationalDegreeTypes(limit, offset, sort, order);
            return Ok(educationalDegreeTypes);
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllEducationalDegreeTypes()
        {
            var educationalDegreeTypes = await _educationalDegreeTypeService.GetAllEducationDegreeTypes();
            return Ok(educationalDegreeTypes);
        }

        [HttpGet]
        [Route("{educationalDegreeType:guid}")]
        public async Task<IActionResult> GetEducationalDegreeType(Guid educationalDegreeType)
        {
            var result = await _educationalDegreeTypeService.GetEducationalDegreeType(educationalDegreeType);
            return Ok(result);
        }

        [HttpPut]
        [Route("{educationalDegreeType:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateEducationalDegreeType(Guid educationalDegreeType, [FromBody]  EducationalDegreeTypeDto educationalDegreeTypeDto)
        {
            await _educationalDegreeTypeService.UpdateEducationalDegreeType(educationalDegreeType, educationalDegreeTypeDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{educationalDegreeType:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteEducationalDegreeType(Guid educationalDegreeType)
        {
            await _educationalDegreeTypeService.DeleteEducationalDegreeType(educationalDegreeType);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateEducationalDegreeType([FromBody] EducationalDegreeTypeDto educationalDegreeTypeDto)
        {
            await _educationalDegreeTypeService.CreateEducationalDegreeType(educationalDegreeTypeDto);
            return StatusCode(201);
        }
    }
}
