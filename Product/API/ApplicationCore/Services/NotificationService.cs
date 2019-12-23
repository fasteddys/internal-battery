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
using UpDiddyLib.Dto;

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

        public async Task<Guid> CreateNotification(Guid subscriberGuid, NewNotificationDto newNotificationDto)
        {
            NotificationDto notificationDto = newNotificationDto.NotificationDto;
            // Ensure required information is present prior to creating new partner
            if (notificationDto == null || string.IsNullOrEmpty(notificationDto.Title) || string.IsNullOrEmpty(notificationDto.Description))
            {
                throw new FailedValidationException("Notifiction information is required");
            }
 
            Guid NewNotificationGuid = Guid.NewGuid();
            DateTime CurrentDateTime = DateTime.UtcNow;
 
            Notification notification = new Notification();
            notification.Title = notificationDto.Title;
            notification.Description = notificationDto.Description;
            notification.NotificationGuid = NewNotificationGuid;
            notification.IsTargeted = notificationDto.IsTargeted == true ? 1 : 0;
            notification.ExpirationDate = notificationDto.ExpirationDate;
            notification.CreateDate = CurrentDateTime;
            notification.ModifyDate = CurrentDateTime;
            notification.IsDeleted = 0;
            notification.ModifyGuid = subscriberGuid;
            notification.CreateGuid = subscriberGuid;
            await _repositoryWrapper.NotificationRepository.Create(notification);
            await _repositoryWrapper.NotificationRepository.SaveAsync();

            Notification NewNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == NewNotificationGuid).Result.FirstOrDefault();
            IList<Subscriber> Subscribers = await _subscriberService.GetSubscribersInGroupAsync(newNotificationDto.GroupGuid);
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CreateSubscriberNotificationRecords(NewNotification, notification.IsTargeted, Subscribers));
            return NewNotificationGuid;
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




    }




}
