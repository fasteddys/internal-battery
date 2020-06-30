using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
            return StatusCode(204);
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
            return StatusCode(204);
        }



        #endregion Employment Preferences

        #region Role Preferences

        [HttpGet("role-preferences")]
        [Authorize]
        public async Task<ActionResult<RolePreferenceDto>> GetRolePreferenceDto()
        {
            var subscriberGuid = GetSubscriberGuid();

            var rolePreferenceDto = await _candidatesService.GetRolePreference(subscriberGuid);
            return rolePreferenceDto;
        }

        [HttpPut("role-preferences")]
        [Authorize]
        public async Task<IActionResult> UpdateRolePreferenceDto(RolePreferenceDto rolePreference)
        {
            var subscriberGuid = GetSubscriberGuid();

            await _candidatesService.UpdateRolePreference(subscriberGuid, rolePreference);
            return StatusCode(204);
        }

        #endregion Role Preferences

        #region Skills 

        [HttpGet("skills")]
        [Authorize]
        public async Task<ActionResult<SkillListDto>> GetSkills(int limit = 10, int offset = 0, string sort = "name", string order = "ascending")
        {
            return Ok(await _candidatesService.GetSkills(GetSubscriberGuid(), limit, offset, sort, order));
        }

        [HttpPut("skills")]
        [Authorize]
        public async Task<IActionResult> UpdateSkills([FromBody] List<string> skillNames)
        {
            await _candidatesService.UpdateSkills(GetSubscriberGuid(), skillNames);
            return StatusCode(204);
        }

        #endregion

        #region Education & Assessments


        [HttpGet]
        [Route("education-assessments")]
        public async Task<IActionResult> GetCandidateEducationAssessments()
        {
            var response = await _candidatesService.GetCandidateEmploymentPreference(GetSubscriberGuid());
            return Ok(response);
        }

        [HttpPut]
        [Route("education-assessments")]
        public async Task<IActionResult> UpdateCandidateEducationAssessments([FromBody] CandidateEmploymentPreferenceDto request)
        {
            await _candidatesService.UpdateCandidateEmploymentPreference(GetSubscriberGuid(), request);
            return StatusCode(204);
        }
        #endregion Education & Assessments
    }
}
