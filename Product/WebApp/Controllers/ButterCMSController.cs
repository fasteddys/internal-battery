using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.Services.ButterCMS;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    [Route("cms")]
    public class ButterCMSController : BaseController
    {
        public IButterCMSService _butterService;

        public ButterCMSController(IApi api,
            IButterCMSService butterService,
            IConfiguration configuration)
            : base(api,configuration)
        {
            _butterService = butterService;
        }
        
        [HttpPost("clear-cached-navigation")]
        public async Task<IActionResult> ClearCachedNavigation()
        {
            if (Request.Headers["CC-Webhook-Unauthenticated"].Count > 0 
                && Request.Headers["CC-Webhook-Unauthenticated"].Equals(_configuration["ButterCMS:WebhookToken"]))
            {
                bool IsCacheCleared = await _butterService.ClearCachedKeyAsync("CareerCirclePublicSiteNavigation");
                if (IsCacheCleared)
                    return Ok(new BasicResponseDto { StatusCode = 200, Description = "ButterCMS Navigation successfully cleared from Redis." });
                else
                    return NotFound(new BasicResponseDto { StatusCode = 500, Description = "An error occurred while attemping to clear cached navigation." });

            }

            return Unauthorized();
        }
    }
}
