using System.Linq;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using System.Collections.Generic;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobFavoriteService : IJobFavoriteService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public JobFavoriteService(IRepositoryWrapper repositoryWrapper) => _repositoryWrapper = repositoryWrapper;

        public async Task AddJobToFavorite(Guid subscriberGuid, Guid jobPostingGuid)
        {
            var jobFavorite = _repositoryWrapper.JobPostingFavorite.GetBySubscriberAndJobPostingGuid(subscriberGuid, jobPostingGuid);
            JobPosting jobPostingEntity = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(jobPostingGuid);
            if (jobPostingEntity == null)
            {
                throw new NotFoundException("Job does not exist");
            }
            Subscriber subsciberEntity = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            JobPostingFavorite jobPostingFavorite = await _repositoryWrapper.JobPostingFavorite.GetBySubscriberAndJobPostingGuid(subscriberGuid, jobPostingGuid);
            if (jobPostingFavorite == null)
            {
                jobPostingFavorite = new JobPostingFavorite()
                {
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = DateTime.UtcNow,
                    IsDeleted = 0,
                    CreateGuid = subsciberEntity.SubscriberGuid.Value,
                    JobPostingFavoriteGuid = Guid.NewGuid(),
                    ModifyGuid = subsciberEntity.SubscriberGuid,
                    SubscriberId = subsciberEntity.SubscriberId,
                    JobPostingId = jobPostingEntity.JobPostingId
                };
                await _repositoryWrapper.JobPostingFavorite.Create(jobPostingFavorite);
                await _repositoryWrapper.JobPostingFavorite.SaveAsync();
            }
            else
            {
                throw new AlreadyExistsException("Job is already added to favorites");
            }
        }

        public async Task DeleteJobFavorite(Guid subscriberGuid, Guid jobPostingGuid)
        {
            JobPostingFavorite jobPostingFavoriteEntity = await _repositoryWrapper.JobPostingFavorite.GetBySubscriberAndJobPostingGuid(subscriberGuid, jobPostingGuid);
            if (jobPostingFavoriteEntity == null)
            {
                throw new NotFoundException("Job favorite not found");
            }
            else
            {
                jobPostingFavoriteEntity.IsDeleted = 1;
                jobPostingFavoriteEntity.ModifyDate = DateTime.UtcNow;
                await _repositoryWrapper.JobPostingFavorite.SaveAsync();
            }
        }

        public async Task<List<JobPostingDto>> GetJobFavorites(Guid subscriberGuid)
        {
            return await _repositoryWrapper.JobPostingFavorite.GetBySubscriberGuid(subscriberGuid);
        }
    }
}

