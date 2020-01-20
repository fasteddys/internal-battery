using AutoMapper;
using Microsoft.EntityFrameworkCore;
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


        // TODO JAB Implement 

        public async Task<bool> SendNotifcation(Guid subscriberGuid, Guid notificationGuid)
        {
           
            // todo jab implment after notification groups 
            /*
            Notification notification = _repositoryWrapper.NotificationRepository.GetAll()
                .Include( g => g.gn)
                .where( not)
                
                GetByConditionAsync(n => n.NotificationGuid == notificationGuid).Result.FirstOrDefault();

            if (notification == null)
                throw new NotFoundException($"Could not find notification {notificationGuid}");


            // adding support for multiple groups
            if (notificationCreateDto.Groups != null && notificationCreateDto.Groups.Count > 0)
            {
                foreach (Guid groupGuid in notificationCreateDto.Groups)
                {
                    IList<Subscriber> Subscribers = await _subscriberService.GetSubscribersInGroupAsync(groupGuid);
                    // Only queue sending the notifications if a valid group which contains members is specified 
                    if (Subscribers != null && Subscribers.Count > 0)
                        _hangfireService.Enqueue<ScheduledJobs>(j => j.CreateSubscriberNotificationRecords(NewNotification, notification.IsTargeted, Subscribers));
                }
            }
            */
            return true;
        }


 
        //todo jab test this endpoint
        public async Task<Guid> CreateNotification(Guid subscriberGuid, NotificationCreateDto notificationCreateDto)
        {
 
            if (notificationCreateDto == null || string.IsNullOrEmpty(notificationCreateDto.Title) || string.IsNullOrEmpty(notificationCreateDto.Description))
            {
                throw new FailedValidationException("Notifiction information is required");
            }
            // Validate the groups 
            List<Group> NotificationGroups = new List<Group>();
            if (notificationCreateDto.Groups != null && notificationCreateDto.Groups.Count > 0)
            {
                foreach (Guid groupGuid in notificationCreateDto.Groups)
                {
                    Group group = await _repositoryWrapper.GroupRepository.GetByGuid(groupGuid);
                    if (group == null)
                        throw new FailedValidationException($"{groupGuid} is not a valid group");
                    NotificationGroups.Add(group);
                }
            }



            Guid NewNotificationGuid = Guid.NewGuid();
            DateTime CurrentDateTime = DateTime.UtcNow;
 
            Notification notification = new Notification();
            notification.Title = notificationCreateDto.Title;
            notification.Description = notificationCreateDto.Description;
            notification.NotificationGuid = NewNotificationGuid;
            notification.IsTargeted = notificationCreateDto.IsTargeted == true ? 1 : 0;
            notification.ExpirationDate = notificationCreateDto.ExpirationDate;
            notification.CreateDate = CurrentDateTime;
            notification.ModifyDate = CurrentDateTime;
            notification.IsDeleted = 0;
            notification.ModifyGuid = subscriberGuid;
            notification.CreateGuid = subscriberGuid;
            await _repositoryWrapper.NotificationRepository.Create(notification);
            await _repositoryWrapper.NotificationRepository.SaveAsync();


            Notification NewNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == NewNotificationGuid).Result.FirstOrDefault();
 
            foreach (Group g in NotificationGroups)
            {
                NotificationGroup newGroup = new NotificationGroup()
                {
                     CreateGuid = Guid.NewGuid(),
                     GroupId = g.GroupId,
                     IsDeleted = 0,
                     NotificationGroupGuid = Guid.NewGuid(),
                     NotificationId = NewNotification.NotificationId
                };

                await _repositoryWrapper.NotificationGroupRepository.Create(newGroup);
            }
           
            await _repositoryWrapper.NotificationGroupRepository.SaveAsync();

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



        public async Task<List<NotificationDto>> GetNotifications(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {    
            List<NotificationDto> rVal = await _repositoryWrapper.StoredProcedureRepository.GetNotifications(limit, offset, sort, order); 
            return rVal;
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
