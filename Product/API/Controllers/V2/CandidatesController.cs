using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;
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

        #region Personal Info

        #endregion Personal Info

        #region Employment Preferences


        [HttpGet]
        [Route("employment-preferences")]
        public async Task<IActionResult> GetCandidateEmploymentPreference()
        {
            var response = await _candidatesService.GetCandidateEmploymentPreference(GetSubscriberGuid());
            return Ok(response);
        }

        [HttpPut]
        [Route("employment-preferences")]
        public async Task<IActionResult> UpdateCandidateEmploymentPreference([FromBody] CandidateEmploymentPreferenceDto request)
        {
            await _candidatesService.UpdateCandidateEmploymentPreference(GetSubscriberGuid(), request);
            return StatusCode(202);
        }



        #endregion Employment Preferences

        #region Role Preferences

        [HttpGet("role-preferences")]
        [Authorize]
        public async Task<ActionResult<Candidate360RoleDto>> GetCandidate360Role()
        {
            var subscriberGuid = GetSubscriberGuid();


            var candidate360Role = await _candidatesService.GetCandidate360Role(subscriberGuid);
            return candidate360Role;
        }

        [HttpPost("role-preferences")]
        [Authorize]
        public async Task<IActionResult> UpdateCandidate360Role(Candidate360RoleDto candidate360Role)
        {
            var subscriberGuid = GetSubscriberGuid();

            await _candidatesService.UpdateCandidate360Role(subscriberGuid, candidate360Role);
            return Ok();
        }

        #endregion Role Preferences
    }
}
