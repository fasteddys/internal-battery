using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Workflow;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class NotificationsController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private IAuthorizationService _authorizationService;
        private ICloudStorage _cloudStorage;
        private IHangfireService _hangfireService;
        private readonly IRepositoryWrapper _repositoryWrapper;


        public NotificationsController(UpDiddyDbContext db,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<SubscriberController> sysLog,
            IDistributedCache distributedCache,
            ICloudStorage cloudStorage,
            IAuthorizationService authorizationService,
            IRepositoryWrapper repositoryWrapper,
            IHangfireService hangfireService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _cloudStorage = cloudStorage;
            _authorizationService = authorizationService;
            _repositoryWrapper = repositoryWrapper;
            _hangfireService = hangfireService;
        }

        [HttpGet]
        public async Task<IList<Notification>> Get()
        {
           return  await _repositoryWrapper.NotificationRepository.GetAllNonDeleted().ToListAsync();

        }

        [HttpGet("{NotificationGuid}")]
        public async Task<Notification> GetNotification(Guid NotificationGuid)
        {
            var result = await _repositoryWrapper.NotificationRepository.GetByConditionAsync(
                s => s.NotificationGuid == NotificationGuid &&
                s.IsDeleted == 0);
            return result.FirstOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDto notificationDto)
        {
            // Ensure required information is present prior to creating new partner
            if (notificationDto == null || string.IsNullOrEmpty(notificationDto.Title) || string.IsNullOrEmpty(notificationDto.Description))
            {
                return BadRequest();
            }

            // Ensure current user is an admin before creating the new partner
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsCareerCircleAdmin");
            if (isAuth.Succeeded)
            {
                Guid NewNotificationGuid = Guid.NewGuid();
                DateTime CurrentDateTime = DateTime.UtcNow;
                Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                Notification notification = new Notification();
                notification.Title = notificationDto.Title;
                notification.Description = notificationDto.Description;
                notification.NotificationGuid = NewNotificationGuid;
                notification.IsTargeted = notificationDto.IsTargeted;
                notification.ExpirationDate = notificationDto.ExpirationDate;
                notification.CreateDate = CurrentDateTime;
                notification.ModifyDate = CurrentDateTime;
                notification.IsDeleted = 0;
                notification.ModifyGuid = loggedInUserGuid;
                notification.CreateGuid = loggedInUserGuid;
                await _repositoryWrapper.NotificationRepository.Create(notification);
                await _repositoryWrapper.NotificationRepository.SaveAsync();

                Notification NewNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == NewNotificationGuid).Result.FirstOrDefault();
                //_hangfireService.Enqueue<ScheduledJobs>(j => j.CreateSubscriberNotificationRecords(NewNotification, notification.IsTargeted, null));
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CreateSubscriberNotificationRecords(NewNotification, notification.IsTargeted, null));
                return Created(_configuration["Environment:ApiUrl"] + "notification/" + notification.NotificationGuid, _mapper.Map<NotificationDto>(notification));
            }
            else
                return Unauthorized();
        }

        [HttpPut]
        public async Task<IActionResult> ModifyNotification([FromBody] NotificationDto NewNotificationDto)
        {
            if (NewNotificationDto == null || NewNotificationDto.NotificationGuid == null)
                return BadRequest();


            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsCareerCircleAdmin");
            if (isAuth.Succeeded)
            {
                Notification ExistingNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == NewNotificationDto.NotificationGuid).Result.FirstOrDefault();
                if (ExistingNotification == null)
                    return NotFound();

                Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                ExistingNotification.Title = NewNotificationDto.Title;
                ExistingNotification.Description = NewNotificationDto.Description;
                ExistingNotification.IsTargeted = NewNotificationDto.IsTargeted;
                ExistingNotification.ExpirationDate = NewNotificationDto.ExpirationDate;
                ExistingNotification.ModifyDate = DateTime.UtcNow;

                _repositoryWrapper.NotificationRepository.Update(ExistingNotification);
                await _repositoryWrapper.NotificationRepository.SaveAsync();

                return Ok(new BasicResponseDto { StatusCode = 200, Description = "Notification " + NewNotificationDto.NotificationGuid + " successfully updated." });
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{NotificationGuid}")]
        public async Task<IActionResult> DeleteNotification(Guid NotificationGuid)
        {
            if (NotificationGuid == null)
                return BadRequest();

            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsCareerCircleAdmin");
            if (isAuth.Succeeded)
            {
                Notification ExistingNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == NotificationGuid).Result.FirstOrDefault();

                if (ExistingNotification == null)
                    return NotFound();

                ExistingNotification.IsDeleted = 1;
                ExistingNotification.ModifyDate = DateTime.UtcNow;

                _repositoryWrapper.NotificationRepository.Update(ExistingNotification);
                await _repositoryWrapper.NotificationRepository.SaveAsync();

                _hangfireService.Enqueue<ScheduledJobs>(j => j.DeleteSubscriberNotificationRecords(ExistingNotification));

                return Ok(new BasicResponseDto { StatusCode = 200, Description = "Notification " + NotificationGuid + " successfully logically deleted." });
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
