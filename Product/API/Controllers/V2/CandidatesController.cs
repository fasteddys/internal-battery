using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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

        #region Language Proficiencies

        [HttpGet("languages")]
        public async Task<ActionResult<LanguageListDto>> GetLanguageList()
            => await _candidatesService.GetLanguageList();

        [HttpGet("language-proficiency-levels")]
        public async Task<ActionResult<ProficiencyLevelListDto>> GetProficiencyLevelList()
            => await _candidatesService.GetProficiencyLevelList();

        [HttpGet("language-proficiency")]
        [Authorize]
        public async Task<ActionResult<LanguageProficiencyListDto>> GetLanguageProficiencies()
        {
            var subscriberGuid = base.GetSubscriberGuid();
            var languagesAndProficiencies = await _candidatesService.GetLanguageProficiencies(subscriberGuid);

            return languagesAndProficiencies;
        }

        [HttpPost("language-proficiency")]
        [Authorize]
        public async Task<IActionResult> CreateLanguageProficiency([FromBody] LanguageProficiencyDto languageProficiency)
        {
            var subscriberGuid = base.GetSubscriberGuid();
            var languageproficiencyguid = await _candidatesService.CreateLanguageProficiency(languageProficiency, subscriberGuid);

            return StatusCode(201, languageproficiencyguid);
        }

        [HttpPut("language-proficiency/{languageproficiencyguid}")]
        [Authorize]
        public async Task<IActionResult> UpdateLanguageProficiency(Guid languageProficiencyGuid, [FromBody] LanguageProficiencyDto languageProficiency)
        {
            var subscriberGuid = base.GetSubscriberGuid();
            await _candidatesService.UpdateLanguageProficiency(languageProficiency, languageProficiencyGuid, subscriberGuid);

            return NoContent();
        }

        [HttpDelete("language-proficiency/{languageproficiencyguid}")]
        [Authorize]
        public async Task<IActionResult> DeleteLanguageProficiency(Guid languageProficiencyGuid)
        {
            var subscriberGuid = base.GetSubscriberGuid();
            await _candidatesService.DeleteLanguageProficiency(languageProficiencyGuid, subscriberGuid);

            return NoContent();
        }

        #endregion Language Proficiencies
    }
}
