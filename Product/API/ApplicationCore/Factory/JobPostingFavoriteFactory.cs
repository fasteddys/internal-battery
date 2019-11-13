using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFavoriteFactory
    {

        public static bool ValidateJobPostingFavorite(IRepositoryWrapper repositoryWrapper, JobPostingFavoriteDto jobPostingFavoriteDto, Guid subsriberGuidClaim, ref Subscriber subscriber, ref JobPosting jobPosting, ref string ErrorMsg)
        {
            // Validate subscriber 
            subscriber = SubscriberFactory.GetSubscriberByGuid(repositoryWrapper, subsriberGuidClaim).Result;
            if (subscriber == null)
            {
                ErrorMsg = $"Subscriber {subsriberGuidClaim} not found";
                return false;
            }

            jobPosting = null;
            // Validate job posting
            if (jobPostingFavoriteDto.JobPosting != null && jobPostingFavoriteDto.JobPosting.JobPostingGuid != null)
                jobPosting = JobPostingFactory.GetJobPostingByGuid(repositoryWrapper, jobPostingFavoriteDto.JobPosting.JobPostingGuid.Value).Result;

            if (jobPosting == null)
            {
                ErrorMsg = $"Job posting not found";
                return false;
            }

            return true;
        }




        public static async Task<List<JobPosting>> GetJobPostingFavoritesForSubscriber(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
            return await repositoryWrapper.JobPostingFavorite.GetAllWithTracking()
                .Include(c => c.Subscriber)
                .Include(c => c.JobPosting)
                .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == guid)
                .OrderByDescending(s => s.CreateDate)
                .Select(jpf => jpf.JobPosting)
                .ToListAsync();
        }



        public static async Task<JobPostingFavorite> GetJobPostingFavoriteByGuidWithRelatedObjects(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
            return await repositoryWrapper.JobPostingFavorite.GetAllWithTracking()
           .Include(c => c.Subscriber)
           .Include(c => c.JobPosting)
           .Where(c => c.IsDeleted == 0 && c.JobPostingFavoriteGuid == guid)
           .FirstOrDefaultAsync();

        }

        public static async Task<JobPostingFavorite> GetJobPostingFavoriteByGuid(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
            return await repositoryWrapper.JobPostingFavorite.GetAllWithTracking()
           .Where(c => c.IsDeleted == 0 && c.JobPostingFavoriteGuid == guid)
           .FirstOrDefaultAsync();

        }
        public static JobPostingFavorite CreateJobPostingFavorite(Subscriber subscriber, JobPosting jobPosting)
        {
            return new JobPostingFavorite()
            {
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                IsDeleted = 0,
                CreateGuid = subscriber.SubscriberGuid.Value,
                JobPostingFavoriteGuid = Guid.NewGuid(),
                ModifyGuid = subscriber.SubscriberGuid,
                SubscriberId = subscriber.SubscriberId,
                JobPostingId = jobPosting.JobPostingId
            };
        }

    }
}
