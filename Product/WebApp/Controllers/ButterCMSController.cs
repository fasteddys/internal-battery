using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.Components.Layout;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    [Authorize(Policy = "IsCareerCircleAdmin")]
    [Route("[controller]")]
    public class ButterCMSController : BaseController
    {
        public IButterCMSService _butterService;

        public ButterCMSController(IApi api,
            IButterCMSService butterService)
            : base(api)
        {
            _butterService = butterService;
        }
        
        [HttpGet("ClearCachedNavigation")]
        public IActionResult ClearCachedNavigation()
        {
            bool IsCacheCleared = _butterService.ClearCachedValue<PublicSiteNavigationViewModel<PublicSiteNavigationMenuItemViewModel>>("CareerCirclePublicSiteNavigation");
            if (IsCacheCleared)
                return Ok(new BasicResponseDto { StatusCode = 200, Description = "ButterCMS Navigation successfully cleared from Redis." });
            else
                return NotFound(new BasicResponseDto { StatusCode = 500, Description = "An error occurred while attemping to clear cached navigation." });
        }
    }
}
