using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class CandidatesController : BaseApiController
    {
        private readonly ICandidatesService _candidatesService;
        private readonly ILogger _logger;

        public CandidatesController(
            ICandidatesService candidatesService,
            ILogger<CandidatesController> logger
            )
        {
            _candidatesService = candidatesService;
            _logger = logger;
        }

        // This empty controller will be used for stories:
        //   #2480 - Candidate 360: Personal Info
        //   #2481 - Candidate 360: Employment Preferences
        //   #2482 - Candidate 360: Role Preferences

        #region Personal Info

        #endregion Personal Info

        #region Employment Preferences

        #endregion Employment Preferences

        #region Role Preferences

        [HttpGet("role-preferences")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<ActionResult<Candidate360RoleDto>> GetCandidate360Role()
        {
            var subscriberGuid = GetSubscriberGuid();

            var candidate360Role = await _candidatesService.GetCandidate360Role(subscriberGuid);
            return candidate360Role;
        }

        [HttpPost("role-preferences")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> UpdateCandidate360Role(Candidate360RoleDto candidate360Role)
        {
            var subscriberGuid = GetSubscriberGuid();

            await _candidatesService.UpdateCandidate360Role(subscriberGuid, candidate360Role);
            return Ok();
        }

        #endregion Role Preferences
    }
}
