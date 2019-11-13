using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/compensation-types/")]
    public class CompensationTypesController : ControllerBase
    {
        private readonly ICompensationTypeService _compensationTypeService;

        public CompensationTypesController(ICompensationTypeService compensationTypeService)
        {
            _compensationTypeService = compensationTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompensationTypes()
        {
            var compensationTypes = await _compensationTypeService.GetCompensationTypes();
            return Ok(compensationTypes);
        }

        [HttpGet]
        [Route("{compensationType}")]
        public async Task<IActionResult> GetCompensationType(Guid compensationType)
        {
            var result  = await _compensationTypeService.GetCompensationType(compensationType);
            return Ok(result);
        }

        [HttpPut]
        [Route("{compensationType}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCompensationType(Guid compensationType, [FromBody]  CompensationTypeDto compensationTypeDto)
        {
            await _compensationTypeService.UpdateCompensationType(compensationType, compensationTypeDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{compensationType}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteCompensationType(Guid compensationType)
        {
            await _compensationTypeService.DeleteCompensationType(compensationType);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateCompensationType([FromBody] CompensationTypeDto compensationTypeDto)
        {
            await _compensationTypeService.CreateCompensationType(compensationTypeDto);
            return StatusCode(201);
        }
       
    }
}
