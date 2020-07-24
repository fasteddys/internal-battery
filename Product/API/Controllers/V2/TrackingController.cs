using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.Controllers.V2
{
    [ApiController]
    [Route("/V2/[controller]/")]
    public class TrackingController : BaseApiController
    {
        private readonly ITrackingService _trackingService;

        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }


        [HttpGet("redirect/{slug}")]
        public async Task<ActionResult<UrlDto>> GetQualifiedUrlAfterTracking(string slug)
        {

            return new UrlDto();
        }

        [HttpPut("landing-page")]
        public async Task<ActionResult> UpdateLandingPageTracking(string fullUrl)
        {

            return StatusCode(202);
        }
    }
}
