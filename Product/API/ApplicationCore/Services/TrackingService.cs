using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
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
        private readonly IJobApplicationService _jobApplicationService;
        private readonly IHangfireService _hangfireService;

        public TrackingService(IRepositoryWrapper repositoryWrapper, IJobApplicationService jobApplicationService, IHangfireService hangfireService)
        {
            _repositoryWrapper = repositoryWrapper;
            _jobApplicationService = jobApplicationService;
            _hangfireService = hangfireService;

        }


        /// <summary>
        /// Gets a mapping of subscribers and list of jobs the subscriber clicked apply to but not submitted at a given date and time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<Dictionary<Subscriber, List<JobPosting>>> GetSubscriberAbandonedJobPostingHistoryByDateAsync(DateTime date)
        {
            IQueryable<SubscriberAction> subscriberAction = _repositoryWrapper.SubscriberActionRepository.GetAll();
            List<SubscriberAction> todaysActions = await subscriberAction
                .Where(x => x.EntityType.Name == UpDiddyLib.Helpers.Constants.EventType.JobPosting && x.Action.Name == UpDiddyLib.Helpers.Constants.Action.ApplyJob && x.CreateDate.Date == date.Date)
                .ToListAsync();
            Dictionary<Subscriber, List<JobPosting>> subscribersToJobPostingMapping = new Dictionary<Subscriber, List<JobPosting>>();
            if (todaysActions.Count > 0)
            {
                var subscribers = todaysActions.Select(x => x.SubscriberId).Distinct().ToList();
                foreach (int sub in subscribers)
                {
                    List<SubscriberAction> actions = todaysActions.Where(x => x.SubscriberId == sub).ToList();
                    Subscriber subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByIdAsync(sub);
                    List<JobPosting> jobPostingList = new List<JobPosting>();
                    foreach (SubscriberAction action in actions)
                    {
                        //Check if subscriber has not submitted the application
                        bool isSubmitted = await _jobApplicationService.IsSubscriberAppliedToJobPosting(subscriber.SubscriberId, action.EntityId.Value);
                        if (!isSubmitted)
                        {
                            JobPosting jobPost = await _repositoryWrapper.JobPosting.GetJobPostingById(action.EntityId.Value);
                            jobPostingList.Add(jobPost);
                        }
                    }
                    //Add to mapping collection only if there is atleast one job the subscriber applied to without submitting
                    if (jobPostingList.Count > 0)
                    {
                        subscribersToJobPostingMapping.Add(subscriber, jobPostingList);
                    }
                }
            }
            return subscribersToJobPostingMapping;
        }

        /// <summary>
        /// Inserts a new record in SubscriberApply table for when subscriber clicks the Apply button in job posting
        /// </summary>
        /// <param name="jobGuid"></param>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task RecordSubscriberApplyActionAsync(Guid jobGuid, Guid subscriberGuid)
        {
            JobPosting jobPosting = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(jobGuid);
            Models.Action action = await _repositoryWrapper.ActionRepository.GetByNameAsync(UpDiddyLib.Helpers.Constants.Action.ApplyJob);
            EntityType entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync(EntityTypeConst.JobPosting);
            Subscriber subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
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
            await _repositoryWrapper.SubscriberActionRepository.Create(subAction);
            await _repositoryWrapper.SubscriberActionRepository.SaveAsync();
        }

        public async Task TrackingSubscriberJobViewAction(Guid jobGuid, Guid subscriberGuid)
        {
            Models.Action action = await _repositoryWrapper.ActionRepository.GetByNameAsync(UpDiddyLib.Helpers.Constants.Action.View);
            EntityType entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync(EntityTypeConst.JobPosting);

            if (jobGuid != Guid.Empty && subscriberGuid != Guid.Empty)
            {
                // invoke the Hangfire job to store the tracking information
                _hangfireService.Enqueue<ScheduledJobs>(j => j.TrackSubscriberActionInformation(subscriberGuid, action.ActionGuid, entityType.EntityTypeGuid, jobGuid));
            }
        }

        public async Task TrackingSubscriberFileDownloadAction(int subscriberId, int fileDownloadTrackerId)
        {
            Models.Action action = await _repositoryWrapper.ActionRepository.GetByNameAsync(UpDiddyLib.Helpers.Constants.Action.DownloadGatedFile);
            EntityType entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync(EntityTypeConst.FileDownloadTracker);
            Subscriber subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByIdAsync(subscriberId);
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
                EntityId = fileDownloadTrackerId,
                ModifyGuid = null,
                OccurredDate = DateTime.UtcNow
            };
            await _repositoryWrapper.SubscriberActionRepository.Create(subAction);
            await _repositoryWrapper.SubscriberActionRepository.SaveAsync();
        }

        public async Task TrackSubscriberAction(int subscriberId, Models.Action action, EntityType entityType, int entityId)
        {
            Subscriber subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByIdAsync(subscriberId);
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
                EntityId = entityId,
                ModifyGuid = null,
                OccurredDate = DateTime.UtcNow
            };
            await _repositoryWrapper.SubscriberActionRepository.Create(subAction);
            await _repositoryWrapper.SubscriberActionRepository.SaveAsync();
        }
    }
}