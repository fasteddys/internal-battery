using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using System.Security.Claims;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Shared.GoogleJobs;
using System.Collections.Generic;
using UpDiddyLib.Helpers;

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


        #region constructor 
        public NotificationsController(IServiceProvider services
        , IJobAlertService jobAlertService
        , IJobFavoriteService jobFavoriteService
        , IJobSearchService jobSearchService
        , ICloudTalentService cloudTalentService
        , ITrackingService trackingService
        , IKeywordService keywordService
        , ISubscriberNotificationService subscriberNotificationService
        ,INotificationService notificationService)


        {
            _services = services;
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _subscriberNotificationService = subscriberNotificationService;
            _notificationService = notificationService;
        }

        #endregion



        [HttpGet("{NotificationGuid}")]
        [Authorize]
        public async Task<IActionResult> GetNotification(Guid NotificationGuid)    
        {
            NotificationDto rVal = null;
            rVal =  await _subscriberNotificationService.GetNotification(GetSubscriberGuid(), NotificationGuid);     
            return Ok(rVal);
        }




        #region notifications

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateNotification([FromBody] NewNotificationDto newNotification)
        {
            Guid rval = await _notificationService.CreateNotification(GetSubscriberGuid(), newNotification);

            return Ok(rval);
        }

        [HttpDelete("{notificationGuid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteNotification (Guid notificationGuid)
        {
             await _notificationService.DeleteNotification(GetSubscriberGuid(), notificationGuid);            
             return StatusCode(204);
        }



        [HttpPut("{notificationGuid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateNotification([FromBody] NotificationDto notificationDto, Guid notificationGuid)
        {
            await _notificationService.UpdateNotification(GetSubscriberGuid(), notificationDto, notificationGuid);
            return StatusCode(204);
        }


        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> GetNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            //  await _notificationService.UpdateNotification(GetSubscriberGuid(), notificationDto, notificationGuid);
            // return StatusCode(204);


            return Ok();
        }




        #endregion




    }
}
