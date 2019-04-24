using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFavoriteFactory
    {
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
