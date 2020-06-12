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

        #endregion Personal Info

        #region Employment Preferences

        [HttpPost]
        [Route("employment-preference/{subscriberGuid}")]
        public async Task<IActionResult> AddCandidateEmploymentPreference(Guid subscriberGuid, [FromBody] CandidateEmploymentPreferenceDto request)
        {
            //await _candidatesService.AddCandidateEmploymentPreference(request);
            return Ok();
        }

        [HttpGet]
        [Route("employment-preference/{subscriberGuid}")]
        public async Task<IActionResult> GetCandidateEmploymentPreference(Guid subscriberGuid)
        {
            //await _candidatesService.GetCandidateEmploymentPreference(request);
            return Ok(new CandidateEmploymentPreferenceDto());
        }

        [HttpPut]
        [Route("employment-preference/{subscriberGuid}")]
        public async Task<IActionResult> UpdateCandidateEmploymentPreference(Guid subscriberGuid, [FromBody] CandidateEmploymentPreferenceDto request)
        {
            //await _candidatesService.UpdateCandidateEmploymentPreference(request);
            return Ok();
        }



        #endregion Employment Preferences

        #region Role Preferences

        #endregion Role Preferences
    }
}
