using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public TrackingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task RecordSubscriberApplyAction(int jobId, int subscriberId)
        {
            SubscriberAction action = new SubscriberAction()
            {
                IsDeleted = 0,
                CreateDate = DateTime.Now,
                ModifyDate = null,
                CreateGuid = Guid.Empty,
                SubscriberId = subscriberId,
                SubscriberActionGuid = Guid.NewGuid(),
                EntityId = jobId,
                EntityTypeId = 1,
                ModifyGuid = null,
                OccurredDate = DateTime.Now

            };
            _repositoryWrapper.SubscriberActionRepository.Create(action);
            await _repositoryWrapper.SubscriberActionRepository.SaveAsync();
        }
    }
}