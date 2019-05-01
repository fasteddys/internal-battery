using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFavoriteFactory
    {

        public static bool ValidateJobPostingFavorite(UpDiddyDbContext db, JobPostingFavoriteDto jobPostingFavoriteDto, Guid subsriberGuidClaim, ref Subscriber subscriber, ref JobPosting jobPosting, ref string ErrorMsg)
        {
            // Validate subscriber 
            subscriber = SubscriberFactory.GetSubscriberByGuid(db, subsriberGuidClaim);
            if (subscriber == null)
            {
                ErrorMsg = $"Subscriber {subsriberGuidClaim} not found";
                return false;
            }

            jobPosting = null;
            // Validate job posting
            if (jobPostingFavoriteDto.JobPosting != null && jobPostingFavoriteDto.JobPosting.JobPostingGuid != null)
                jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobPostingFavoriteDto.JobPosting.JobPostingGuid.Value);

            if (jobPosting == null)
            {
                ErrorMsg = $"Job posting not found";
                return false;
            }

            return true;
        }




        public static List<JobPosting> GetJobPostingFavoritesForSubscriber(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPostingFavorite
                .Include(c => c.Subscriber)
                .Include(c => c.JobPosting)
                .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == guid)
                .OrderByDescending(s => s.CreateDate)
                .Select(jpf => jpf.JobPosting)
                .ToList();
        }



        public static JobPostingFavorite GetJobPostingFavoriteByGuidWithRelatedObjects(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPostingFavorite
           .Include(c => c.Subscriber)
           .Include(c => c.JobPosting)
           .Where(c => c.IsDeleted == 0 && c.JobPostingFavoriteGuid == guid)
           .FirstOrDefault();

        }

        public static JobPostingFavorite GetJobPostingFavoriteByGuid(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPostingFavorite
           .Where(c => c.IsDeleted == 0 && c.JobPostingFavoriteGuid == guid)
           .FirstOrDefault();

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
