using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/employment-types/")]
    public class EmploymentTypesController : ControllerBase
    {
        private readonly IEmploymentTypeService _employmentTypeService;

        public EmploymentTypesController(IEmploymentTypeService employmentTypeService)
        {
            _employmentTypeService = employmentTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmploymentTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var employmentTypes = await _employmentTypeService.GetEmploymentTypes(limit, offset, sort, order);
            return Ok(employmentTypes);
        }

        [HttpGet]
        [Route("{employmentType:guid}")]
        public async Task<IActionResult> GetEmploymentType(Guid employmentType)
        {
            var result = await _employmentTypeService.GetEmploymentType(employmentType);
            return Ok(result);
        }

        [HttpPut]
        [Route("{employmentType:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateEmploymentType(Guid employmentType, [FromBody]  EmploymentTypeDto employmentTypeDto)
        {
            await _employmentTypeService.UpdateEmploymentType(employmentType, employmentTypeDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{employmentType:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteEmploymentType(Guid employmentType)
        {
            await _employmentTypeService.DeleteEmploymentType(employmentType);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateEmploymentType([FromBody] EmploymentTypeDto employmentTypeDto)
        {
            await _employmentTypeService.CreateEmploymentType(employmentTypeDto);
            return StatusCode(201);
        }
    }
}
