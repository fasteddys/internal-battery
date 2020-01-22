using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [Authorize(Policy = "IsCareerCircleAdmin")]
    public class PartnersController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnersController(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPartners(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var partners = await _partnerService.GetPartners(limit, offset, sort, order);
            return Ok(partners);
        }

        [HttpGet]
        [Route("{partner:guid}")]
        public async Task<IActionResult> GetPartner(Guid partner)
        {
            var result  = await _partnerService.GetPartner(partner);
            return Ok(result);
        }

        [HttpPut]
        [Route("{partner:guid}")]
        public async Task<IActionResult> UpdatePartner(Guid partner, [FromBody]  PartnerDto partnerDto)
        {
            await _partnerService.UpdatePartner(partner, partnerDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{partner:guid}")]
        public async Task<IActionResult> DeletePartner(Guid partner)
        {
            await _partnerService.DeletePartner(partner);
            return StatusCode(204);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePartner([FromBody] PartnerDto partnerDto)
        {
            await _partnerService.CreatePartner(partnerDto);
            return StatusCode(201);
        }
       
    }
}
