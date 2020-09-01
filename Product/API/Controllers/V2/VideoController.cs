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

        [HttpGet("{isPreview:bool}")]
        public async Task<ActionResult<SubscriberVideoLinksDto>> GetSubscriberVideoLink(bool isPreview)
        {
            var subscriberGuid = GetSubscriberGuid();
            var videoLink = await _videoService.GetSubscriberVideoLink(subscriberGuid, isPreview);
            return videoLink;
        }

        [HttpPost("{redisKeyGuid}")]
        public async Task<IActionResult> SetSubscriberVideoLink(Guid redisKeyGuid, [FromBody] SubscriberVideoLinksDto subscriberVideo)
        {
            var subscriberVideoGuid = await GetSubscriberVideoGuidFromRedis(redisKeyGuid);
            await _videoService.SetSubscriberVideoLink(subscriberVideoGuid, subscriberVideo);
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpDelete("{subscriberVideoGuid}")]
        [Authorize]
        public async Task<IActionResult> DeleteSubscriberVideoLink(Guid subscriberVideoGuid)
        {
            var subscriberGuid = GetSubscriberGuid();

            await _videoService.DeleteSubscriberVideoLink(subscriberVideoGuid, subscriberGuid);
            return NoContent();
        }

        [HttpPut("publish/{subscriberVideoGuid}")]
        [Authorize]
        public async Task<IActionResult> TogglePublish(Guid subscriberVideoGuid)
        {
            var subscriberGuid = GetSubscriberGuid();

            await _videoService.Publish(subscriberVideoGuid, subscriberGuid, true);
            return NoContent();
        }

        [HttpPut("hm-visibility")]
        [Authorize]
        public async Task<IActionResult> SetVideoIsVisibleToHiringManager([FromQuery]bool visibility)
        {
            var subscriberGuid = GetSubscriberGuid();
            await _videoService.SetVideoIsVisibleToHiringManager(subscriberGuid, visibility);
            return NoContent();
        }

        private async Task<Guid> GetSubscriberVideoGuidFromRedis(Guid redisKeyGuid)
        {
            var subscriberVideoId = await _redis.GetStringAsync(redisKeyGuid.ToString());

            if (string.IsNullOrEmpty(subscriberVideoId) || !Guid.TryParse(subscriberVideoId, out var subscriberVideoGuid))
            {
                throw new NotAuthorizedException();
            }

            return subscriberVideoGuid;
        }
    }
}
