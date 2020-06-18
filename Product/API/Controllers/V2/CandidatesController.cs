using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.Controllers.V2
{
    [Route("v2/[controller]/")]
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

        [HttpGet]
        [Route("personal-information")]
        public async Task<IActionResult> GetCandidatePersonalInfo()
        {
            var response = await _candidatesService.GetCandidatePersonalInfo(GetSubscriberGuid());
            return Ok(response);
        }

        [HttpPut]
        [Route("personal-information")]
        public async Task<IActionResult> UpdateCandidatePersonalInfo([FromBody] CandidatePersonalInfoDto request)
        {
            await _candidatesService.UpdateCandidatePersonalInfo(GetSubscriberGuid(), request);
            return StatusCode(202);
        }
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

        #endregion Role Preferences
    }
}
