using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.Components.Layout;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    [Route("cms")]
    public class ButterCMSController : BaseController
    {
        public IButterCMSService _butterService;
        public IConfiguration _configuration;

        public ButterCMSController(IApi api,
            IButterCMSService butterService,
            IConfiguration configuration)
            : base(api)
        {
            _butterService = butterService;
            _configuration = configuration;
        }
        
        [HttpPost("clear-cached-navigation")]
        public async Task<IActionResult> ClearCachedNavigation()
        {
            if (Request.Headers["CC-Webhook-Unauthenticated"].Count > 0 
                && Request.Headers["CC-Webhook-Unauthenticated"].Equals(_configuration["ButterCMS:WebhookToken"]))
            {
                bool IsCacheCleared = await _butterService.ClearCachedValueAsync<PublicSiteNavigationViewModel<PublicSiteNavigationMenuItemViewModel>>("CareerCirclePublicSiteNavigation");
                if (IsCacheCleared)
                    return Ok(new BasicResponseDto { StatusCode = 200, Description = "ButterCMS Navigation successfully cleared from Redis." });
                else
                    return NotFound(new BasicResponseDto { StatusCode = 500, Description = "An error occurred while attemping to clear cached navigation." });

            }

            return Unauthorized();
        }
    }
}
