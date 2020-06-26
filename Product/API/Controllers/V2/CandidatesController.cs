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

        [HttpGet("language-proficiencies")]
        public async Task<ActionResult<ProficiencyLevelListDto>> GetProficiencyList()
            => await _candidatesService.GetProficiencyList();

        [HttpGet("language-fluency")]
        [Authorize]
        public async Task<ActionResult<LanguageProficiencyListDto>> GetLanguagesAndProficiencies()
        {
            var subscriberGuid = base.GetSubscriberGuid();
            var languagesAndProficiencies = await _candidatesService.GetLanguagesAndProficiencies(subscriberGuid);

            return languagesAndProficiencies;
        }

        [HttpPut("language-fluency")]
        [Authorize]
        public async Task<IActionResult> UpdateLanguagesAndProficiencies(LanguageProficiencyListDto languagesAndProficiencies)
        {
            var subscriberGuid = base.GetSubscriberGuid();

            await _candidatesService.UpdateLanguagesAndProficiencies(languagesAndProficiencies, subscriberGuid);

            return NoContent();
        }

        #endregion Language Proficiencies
    }
}
