using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/industries/")]
    public class IndustriesController : ControllerBase
    {
        private readonly IIndustryService _industryService;

        public IndustriesController(IIndustryService industryService)
        {
            _industryService = industryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetIndustries(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var Industrys = await _industryService.GetIndustries(limit, offset, sort, order);
            return Ok(Industrys);
        }

        [HttpGet]
        [Route("{industry:guid}")]
        public async Task<IActionResult> GetIndustry(Guid industry)
        {
            var result = await _industryService.GetIndustry(industry);
            return Ok(result);
        }

        [HttpPut]
        [Route("{industry:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateIndustry(Guid industry, [FromBody] IndustryDto industryDto)
        {
            await _industryService.UpdateIndustry(industry, industryDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{Industry:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteIndustry(Guid industry)
        {
            await _industryService.DeleteIndustry(industry);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateIndustry([FromBody] IndustryDto industryDto)
        {
            var industryGuid = await _industryService.CreateIndustry(industryDto);
            return StatusCode(201, industryGuid);
        }
    }
}
