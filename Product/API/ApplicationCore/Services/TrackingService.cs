using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using EntityTypeConst = UpDiddyLib.Helpers.Constants.EventType;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public TrackingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        /// <summary>
        /// Inserts a new record in SubscriberApply table for when subscriber clicks the Apply button in job posting
        /// </summary>
        /// <param name="jobGuid"></param>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task RecordSubscriberApplyAction(Guid jobGuid, Guid subscriberGuid)
        {
            JobPosting jobPosting = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(jobGuid);
            Models.Action action = await _repositoryWrapper.ActionRepository.GetByNameAsync(UpDiddyLib.Helpers.Constants.Action.ApplyJob);
            EntityType entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync(EntityTypeConst.JobPosting);
            Subscriber subscriber = await _repositoryWrapper.Subscriber.GetSubscriberByGuidAsync(subscriberGuid);
            SubscriberAction subAction = new SubscriberAction()
            {
                IsDeleted = 0,
                CreateDate = DateTime.UtcNow,
                ModifyDate = null,
                Action = action,
                CreateGuid = Guid.Empty,
                Subscriber = subscriber,
                SubscriberActionGuid = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = jobPosting.JobPostingId,
                ModifyGuid = null,
                OccurredDate = DateTime.UtcNow
            };
            _repositoryWrapper.SubscriberActionRepository.Create(subAction);
            await _repositoryWrapper.SubscriberActionRepository.SaveAsync();
        }
    }
}