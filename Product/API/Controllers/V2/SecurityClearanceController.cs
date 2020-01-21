using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/security-clearances/")]
    public class SecurityClearanceController : ControllerBase
    {
        private readonly ISecurityClearanceService _securityClearanceService;

        public SecurityClearanceController(ISecurityClearanceService securityClearanceService)
        {
            _securityClearanceService = securityClearanceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSecurityClearances(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var securityClearances = await _securityClearanceService.GetSecurityClearances(limit, offset, sort, order);
            return Ok(securityClearances);
        }

        [HttpGet]
        [Route("{securityClearance:guid}")]
        public async Task<IActionResult> GetSecurityClearance(Guid securityClearance)
        {
            var result  = await _securityClearanceService.GetSecurityClearance(securityClearance);
            return Ok(result);
        }

        [HttpPut]
        [Route("{securityClearance:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateSecurityClearance(Guid securityClearance, [FromBody]  SecurityClearanceDto securityClearanceDto)
        {
            await _securityClearanceService.UpdateSecurityClearance(securityClearance, securityClearanceDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{securityClearance:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteSecurityClearance(Guid securityClearance)
        {
            await _securityClearanceService.DeleteSecurityClearance(securityClearance);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateSecurityClearance([FromBody] SecurityClearanceDto securityClearanceDto)
        {
            await _securityClearanceService.CreateSecurityClearance(securityClearanceDto);
            return StatusCode(201);
        }
       
    }
}
