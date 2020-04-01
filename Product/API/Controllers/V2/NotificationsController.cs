using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/notifications/")]
    [ApiController]
    public class NotificationsController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IServiceProvider _services;
        private readonly ISubscriberNotificationService _subscriberNotificationService;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;

        #region constructor 

        public NotificationsController(IServiceProvider services
        , IJobAlertService jobAlertService
        , IJobFavoriteService jobFavoriteService
        , IJobSearchService jobSearchService
        , ICloudTalentService cloudTalentService
        , ITrackingService trackingService
        , IKeywordService keywordService
        , ISubscriberNotificationService subscriberNotificationService
        , INotificationService notificationService
        , IAuthorizationService authorizationService)
        {
            _services = services;
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _subscriberNotificationService = subscriberNotificationService;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
        }

        #endregion

        #region subscriber notifications

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("{notificationGuid}/subscribers/{subscriberGuid}")]
        public async Task<IActionResult> CreateSubscriberNotification(Guid NotificationGuid, Guid subscriberGuid)
        {
            var subscriberNotificationGuid = await _subscriberNotificationService.CreateSubscriberNotification(GetSubscriberGuid(), NotificationGuid, subscriberGuid);
            return StatusCode(201, subscriberNotificationGuid);
        }

        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("{notificationGuid}/subscribers/{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberNotification(Guid NotificationGuid, Guid subscriberGuid)
        {
            bool rval = await _subscriberNotificationService.DeleteSubscriberNotification(subscriberGuid, NotificationGuid);
            return StatusCode(204);
        }

        [HttpDelete]
        [Authorize]
        [Route("{notificationGuid}/subscribers/")]
        public async Task<IActionResult> DeleteSubscriberNotification(Guid NotificationGuid)
        {
            bool rval = await _subscriberNotificationService.DeleteSubscriberNotification(GetSubscriberGuid(), NotificationGuid);
            return StatusCode(204);
        }


        [HttpPut]
        [Authorize]
        [Route("{notificationGuid}/subscribers/")]
        public async Task<IActionResult> UpdateSubscriberNotification([FromBody] NotificationDto notification, Guid NotificationGuid)
        {
            bool rval = await _subscriberNotificationService.UpdateSubscriberNotification(GetSubscriberGuid(), NotificationGuid, notification);
            return StatusCode(200);
        }

        [HttpGet]
        [Authorize]
        [Route("subscribers")]
        public async Task<IActionResult> GetSubscriberNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var rval = await _subscriberNotificationService.GetNotifications(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(rval);
        }

        #endregion

        #region notifications

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto newNotification)
        {
            var notificationGuid = await _notificationService.CreateNotification(GetSubscriberGuid(), newNotification);
            return StatusCode(201, notificationGuid);
        }

        [HttpDelete("{notificationGuid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteNotification(Guid notificationGuid)
        {
            await _notificationService.DeleteNotification(GetSubscriberGuid(), notificationGuid);
            return StatusCode(204);
        }

        [HttpPut("{notificationGuid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateNotification([FromBody] NotificationCreateDto notification, Guid notificationGuid)
        {
            await _notificationService.UpdateNotification(GetSubscriberGuid(), notification, notificationGuid);
            return StatusCode(200);
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> GetNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            NotificationListDto rVal = _mapper.Map<NotificationListDto>(await _notificationService.GetNotifications(limit, offset, sort, order));
            return Ok(rVal);
        }

        [HttpGet("{notificationGuid}")]
        [Authorize]
        public async Task<IActionResult> GetSubscriberNotification(Guid notificationGuid)
        {
            NotificationDto rVal = null;
            rVal = await _notificationService.GetNotification(notificationGuid);
            return Ok(rVal);
        }

        // send notification 
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("{notificationGuid}")]
        public async Task<IActionResult> SendNotification(Guid notificationGuid)
        {
            await _notificationService.SendNotifcation(GetSubscriberGuid(), notificationGuid);
            return StatusCode(202);
        }


        #endregion
    }
}
