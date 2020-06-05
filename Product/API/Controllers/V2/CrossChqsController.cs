using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class CrossChqsController : BaseApiController
    {
        private readonly ICrosschqService _crosschqService;

        public CrossChqsController(ICrosschqService crosschqService)
        {
            _crosschqService = crosschqService;
        }

        [HttpGet("references/{profileGuid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> ReferenceRequest(Guid profileGuid)
        {
            var response = await _crosschqService
                .RetrieveReferenceStatus(profileGuid, GetSubscriberGuid());

            return Ok(response);
        }

        [HttpPost("references/{profileGuid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> ReferenceRequest(Guid profileGuid, [FromBody] CrossChqReferenceRequestDto referenceRequest)
        {
            var requestId = await _crosschqService
                .CreateReferenceRequest(profileGuid, GetSubscriberGuid(), referenceRequest);

            return Ok(requestId);
        }
    }
}
