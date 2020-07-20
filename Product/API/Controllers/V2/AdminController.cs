using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    [Route("/V2/[controller]/")]
    public class AdminController : BaseApiController
    {
        private readonly IAccountManagementService _accountManagementService;

        public AdminController(IAccountManagementService accountManagementService)
        {
            _accountManagementService = accountManagementService;
        }

        [HttpGet("subscriber/lookup-by-email")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<ActionResult<UserStatsDto>> GetUserStatsByEmail([FromQuery]string email)
        {
            var userStats = await _accountManagementService.GetUserStatsByEmail(email);
            if (userStats == null) { return NotFound(); }
            return userStats;
        }

        [HttpGet("subscriber/{subscriber}/auth0-verification-status")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<ActionResult<bool>> GetAuth0VerificationStatus(Guid subscriber)
        {
            bool isVerified = await _accountManagementService.GetAuth0VerificationStatus(subscriber);
            return isVerified;
        }

        [HttpPut("subscriber/{subscriber}/force-verification")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> ForceVerification(Guid subscriber)
        {
            await _accountManagementService.ForceVerification(subscriber);
            return StatusCode(202);
        }

        [HttpPost("subscriber/{subscriber}/send-verification-email")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> SendVerificationEmail(Guid subscriber)
        {
            await _accountManagementService.SendVerificationEmail(subscriber);
            return StatusCode(202);
        }

        [HttpDelete("subscriber/{subscriber}/delete")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> RemoveAccount(Guid subscriber)
        {
            await _accountManagementService.RemoveAccount(subscriber);
            return NoContent();
        }
    }
}
