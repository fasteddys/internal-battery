using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly ISubscriberService _subscriberService;
        private readonly IHangfireService _hangfireService;

        public NotificationService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration, ISubscriberService subscriberService, IHangfireService hangfireService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = configuration;
            _subscriberService = subscriberService;
            _hangfireService = hangfireService;
        }

        public async Task<Guid> CreateNotification(Guid subscriberGuid, NotificationDto notificationDto, Guid? groupGuid = null)
        {
            // Ensure required information is present prior to creating new partner
            if (notificationDto == null || string.IsNullOrEmpty(notificationDto.Title) || string.IsNullOrEmpty(notificationDto.Description))
            {
                throw new FailedValidationException("Notifiction information is required");
            }
 
            Notification notification = new Notification();
            notification.Title = notificationDto.Title;
            notification.Description = notificationDto.Description;
            notification.NotificationGuid = Guid.NewGuid();
            notification.IsTargeted = notificationDto.IsTargeted == true ? 1 : 0;
            notification.ExpirationDate = notificationDto.ExpirationDate;
            notification.CreateDate = DateTime.UtcNow;
            notification.IsDeleted = 0;
            notification.CreateGuid = Guid.Empty;
            await _repositoryWrapper.NotificationRepository.Create(notification);
            await _repositoryWrapper.NotificationRepository.SaveAsync();

            Notification newNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == notification.NotificationGuid).Result.FirstOrDefault();
            IList<Subscriber> Subscribers = await _subscriberService.GetSubscribersInGroupAsync(groupGuid);
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CreateSubscriberNotificationRecords(newNotification, Subscribers));
            return notification.NotificationGuid;
        }

        public async Task DeleteNotification(Guid subscriberGuid, Guid notificationGuid)
        {
            if (notificationGuid == null)
                throw new FailedValidationException("Notification guid is required");
    
            Notification ExistingNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == notificationGuid).Result.FirstOrDefault();

            if (ExistingNotification == null)
                throw new NotFoundException($"Unable to location notification {notificationGuid}");

            ExistingNotification.IsDeleted = 1;
            ExistingNotification.ModifyDate = DateTime.UtcNow;

            _repositoryWrapper.NotificationRepository.Update(ExistingNotification);
                await _repositoryWrapper.NotificationRepository.SaveAsync();

            _hangfireService.Enqueue<ScheduledJobs>(j => j.DeleteSubscriberNotificationRecords(ExistingNotification));

            return;
                   
        }

        public async Task UpdateNotification(Guid subscriberGuid, NotificationDto notification, Guid notificationGuid)
        {
            if (notification == null)
                throw new FailedValidationException("Notification update information is required");

            Notification ExistingNotification = await _repositoryWrapper.NotificationRepository.GetByGuid(notificationGuid);

            if (ExistingNotification == null)
                throw new NotFoundException($"Cannot find notification {notificationGuid}");
 

            ExistingNotification.Title = notification.Title;
            ExistingNotification.Description = notification.Description;
            ExistingNotification.IsTargeted = notification.IsTargeted == true ? 1 : 0;
            ExistingNotification.ExpirationDate = notification.ExpirationDate;
            ExistingNotification.ModifyDate = DateTime.UtcNow;

            _repositoryWrapper.NotificationRepository.Update(ExistingNotification);
            await _repositoryWrapper.NotificationRepository.SaveAsync();

            return;
        
        }

        public async Task<NotificationListDto> GetNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {    
            List<NotificationDto> notifications = await _repositoryWrapper.StoredProcedureRepository.GetNotifications(limit, offset, sort, order);
            return _mapper.Map<NotificationListDto>(notifications);
        }
        
        public async Task<NotificationDto> GetNotification(Guid notificationGuid)
        {
            if (notificationGuid == null)
                throw new FailedValidationException("Notification guid is required");

            Notification ExistingNotification = await _repositoryWrapper.NotificationRepository.GetByGuid(notificationGuid);

            if (ExistingNotification == null)
                throw new NotFoundException($"Cannot find notification {notificationGuid}");
 
            return _mapper.Map<NotificationDto>(ExistingNotification);
        }
    }
}
