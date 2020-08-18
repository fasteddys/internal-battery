using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Net;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class VideoController : BaseApiController
    {
        private readonly IVideoService _videoService;
        private readonly IRedisService _redis;

        public VideoController(IVideoService videoService, IRedisService redis)
        {
            _videoService = videoService;
            _redis = redis;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<SubscriberVideoLinksDto>> GetSubscriberVideoLink()
        {
            var subscriberGuid = GetSubscriberGuid();
            var videoLink = await _videoService.GetSubscriberVideoLink(subscriberGuid);
            return videoLink;
        }

        [HttpPost("{redisKeyGuid}")]
        public async Task<IActionResult> SetSubscriberVideoLink(Guid redisKeyGuid, [FromBody] SubscriberVideoLinksDto subscriberVideo)
        {
            var subscriberGuid = await GetSubscriberGuidFromRedis(redisKeyGuid);
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

        private async Task<Guid> GetSubscriberGuidFromRedis(Guid redisKeyGuid)
        {
            var subscriberId = await _redis.GetStringAsync(redisKeyGuid.ToString());

            if (string.IsNullOrEmpty(subscriberId) || !Guid.TryParse(subscriberId, out var subscriberGuid))
            {
                throw new NotAuthorizedException();
            }

            return subscriberGuid;
        }
    }
}
