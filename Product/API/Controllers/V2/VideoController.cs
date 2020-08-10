using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class VideoController : BaseApiController
    {
        private readonly IVideoService _videoService;

        public VideoController(IVideoService videoService)
        {
            _videoService = videoService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<SubscriberVideoLinksDto>> GetSubscriberVideoLink()
        {
            var subscriberGuid = GetSubscriberGuid();
            var videoLink = await _videoService.GetSubscriberVideoLink(subscriberGuid);
            return videoLink;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetSubscriberVideoLink([FromBody] SubscriberVideoLinksDto subscriberVideo)
        {
            var subscriberGuid = GetSubscriberGuid();
            await _videoService.SetSubscriberVideoLink(subscriberGuid, subscriberVideo);
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteSubscriberVideoLink()
        {
            var subscriberGuid = GetSubscriberGuid();
            await _videoService.DeleteSubscriberVideoLink(subscriberGuid);
            return NoContent();
        }

        [HttpGet("hm-visibility")]
        [Authorize]
        public async Task<ActionResult<bool>> GetVideoIsVisibleToHiringManager()
        {
            var subscriberGuid = GetSubscriberGuid();
            var isVisibleToHiringManager = await _videoService.GetVideoIsVisibleToHiringManager(subscriberGuid);
            return isVisibleToHiringManager;
        }

        [HttpPut("hm-visibility")]
        [Authorize]
        public async Task<IActionResult> SetVideoIsVisibleToHiringManager([FromQuery]bool visibility)
        {
            var subscriberGuid = GetSubscriberGuid();
            await _videoService.SetVideoIsVisibleToHiringManager(subscriberGuid, visibility);
            return NoContent();
        }
    }
}
