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
        private readonly IProfileService _profileService;
        private readonly IRecruiterService _recruiterService;

        public CrossChqsController(
            ICrosschqService crosschqService,
            IProfileService profileService,
            IRecruiterService recruiterService)
        {
            _crosschqService = crosschqService;
            _profileService = profileService;
            _recruiterService = recruiterService;
        }

        [HttpGet("references/{profileGuid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> ReferenceRequest(Guid profileGuid)
        {
            var profile = await _profileService
                .GetProfileForRecruiter(profileGuid, GetSubscriberGuid());

            var response = await _crosschqService
                .RetrieveReferenceStatus(profile);

            return Ok(response);
        }

        [HttpPost("references/{profileGuid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> ReferenceRequest(Guid profileGuid, [FromBody] CrossChqReferenceRequestDto referenceRequest)
        {
            var subscriberId = GetSubscriberGuid();

            var profile = await _profileService
                .GetProfileForRecruiter(profileGuid, subscriberId);

            var recruiter = await _recruiterService
                .GetRecruiterBySubscriberAsync(subscriberId);

            var requestId = await _crosschqService
                .CreateReferenceRequest(profile, recruiter, referenceRequest);

            return Ok(requestId);
        }
    }
}
