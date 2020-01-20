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
        ,INotificationService notificationService
        , IAuthorizationService authorizationService    )


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
        [Route("{notificationGuid}/subscribers/{subscriberGuid}") ]
        public async Task<IActionResult> CreateSubscriberNotification( Guid NotificationGuid, Guid subscriberGuid)
        {
            Guid rval = await _subscriberNotificationService.CreateSubscriberNotification(GetSubscriberGuid(), NotificationGuid, subscriberGuid);
            return StatusCode(201);
        }

        
        [HttpDelete]
        [Authorize]
        [Route("{notificationGuid}/subscribers/{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberNotification(Guid NotificationGuid, Guid subscriberGuid)
        {
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsCareerCircleAdmin");

            bool rval = await _subscriberNotificationService.DeleteSubscriberNotification(isAuth.Succeeded, GetSubscriberGuid(), NotificationGuid, subscriberGuid);
            return StatusCode(201);
        }




        [HttpPut]
        [Authorize]
        [Route("{notificationGuid}/subscribers/{subscriberGuid}")]
        public async Task<IActionResult> UpdateSubscriberNotification([FromBody] NotificationDto notification, Guid NotificationGuid, Guid subscriberGuid)
        {
            bool rval = await _subscriberNotificationService.UpdateSubscriberNotification(GetSubscriberGuid(), NotificationGuid, subscriberGuid, notification);
            return StatusCode(201);
        }
 
        [HttpGet]
        [Authorize]
        [Route("subscribers")]
        public async Task<IActionResult> GetSubscriberNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var  rval = await _subscriberNotificationService.GetNotifications(GetSubscriberGuid(),limit,offset,sort,order);
            return Ok(rval);
        }




        #endregion



        #region notifications

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto newNotification)
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

       // todo jab would you also take a look at section 19.3 question 1? 
       // TODO JAB
       /*
        * 
        * 
        * 
        * Hi Koenig, Bill and Brazil, Jim, thanks for the update on the endpoints. The Job Admin - 
        * List is returning 9444 of total records available but there are only 15,
        * I added the list of talents guids that last week was provided with the real guids to the account dfrayo@hellobuild.co as talent favorites,
        * the endpoint Talent favorite by subscriber List is returning the correct data but the endpoint Talent Favorite - 
        * Delete is returning 404 for all the guids provided and previous added as favorite fot this account. Could you please check the two endpoints
        */

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
            List<NotificationDto> rVal = await _notificationService.GetNotifications(limit, offset, sort, order);            
            return Ok(rVal);
        }


        [HttpGet("{NotificationGuid}")]
        [Authorize]
        public async Task<IActionResult> GetSubscriberNotification(Guid NotificationGuid)
        {
            NotificationDto rVal = null;
            rVal = await _notificationService.GetNotification(NotificationGuid);
            return Ok(rVal);
        }
        #endregion




    }
}
