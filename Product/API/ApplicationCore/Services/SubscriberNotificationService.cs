using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberNotificationService : ISubscriberNotificationService
    {
        private ILogger<SubscriberNotificationService> _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }

        public SubscriberNotificationService(ILogger<SubscriberNotificationService> logger, IRepositoryWrapper repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<bool> DeleteSubscriberNotification(Guid subscriberGuid, Guid notificationGuid)
        {
            bool isOperationSuccessful = false;
            try
            {
                var subscriberNotification = await _repository.SubscriberNotificationRepository.GetSubscriberNotificationByIdentifiersAsync(subscriberGuid, notificationGuid);
                if (subscriberNotification == null)
                    throw new ApplicationException($"Unrecognized subscriber notification; subscriberGuid: {subscriberGuid}, notificationGuid: {notificationGuid}");
                subscriberNotification.IsDeleted = 1;
                subscriberNotification.ModifyDate = DateTime.UtcNow;
                subscriberNotification.ModifyGuid = Guid.Empty;
                await _repository.SubscriberNotificationRepository.SaveAsync();
                isOperationSuccessful = true;
            }
            catch(Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberNotificationService.DeleteSubscriberNotification: An error occured while attempting to delete the subscriber notification. Message: {e.Message}", e);
            }
            return isOperationSuccessful;
        }
    }
}
