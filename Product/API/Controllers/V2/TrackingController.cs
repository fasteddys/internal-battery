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

        [HttpGet("redirect/{slug:length(1,150)}")]
        public async Task<ActionResult<UrlDto>> GetQualifiedUrlAfterTracking(string slug)
        {
            var response = await _trackingService.GetFullUrlAfterTracking(slug);

            if(response == null) return NotFound();

            return response;
        }

        [HttpPut("landing-page")]
        public async Task<ActionResult> AddUpdateLandingPageTracking(UrlDto urlDto)
        {
            await _trackingService.AddUpdateLandingPageTracking(urlDto.Url.GetLeftPart(UriPartial.Path));
            return StatusCode(202);
        }
    }
}
