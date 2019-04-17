using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore
{
    public class JobApplicationFactory
    {


        public static List<JobApplication> GetJobApplicationsForSubscriber(UpDiddyDbContext db, int subscriberId)
        {
            return db.jobApplication
                .Include( s => s.JobPosting)
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .ToList();
        }


        public static List<JobApplication> GetJobApplicationsForPosting(UpDiddyDbContext db, int jobPostingID)
        {
            return db.jobApplication                
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingID)       
                .ToList();
        }


        public static JobApplication GetJobApplicationByGuid(UpDiddyDbContext db, Guid jobApplicationGuid)
        {
            return db.jobApplication
                .Include(s => s.JobPosting)
                .Include(s => s.Subscriber)
                .Where(s => s.IsDeleted == 0  && s.JobApplicationGuid == jobApplicationGuid)                   
                .FirstOrDefault();
        }


        public static JobApplication GetJobApplication(UpDiddyDbContext db, int subscriberId, int jobPostingID)
        {
            return db.jobApplication
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingID && s.SubscriberId == subscriberId)
                .FirstOrDefault();
        }

        static public bool ValidateJobApplication(UpDiddyDbContext db, JobApplicationDto jobApplicationDto, ref Subscriber subscriber, ref JobPosting jobPosting, ref int ErrorCode, ref string ErrorMsg)
        {

            if (jobApplicationDto == null )
            {
                ErrorCode = 400;
                ErrorMsg = "Job application is required";
                return false;
            }



            //validate subscriber 
            if (jobApplicationDto.Subscriber == null || jobApplicationDto.Subscriber.SubscriberGuid == null)
            {
                ErrorCode = 400;
                ErrorMsg = "No subscriber has been specified";
                return false;
            }
                        
            subscriber = SubscriberFactory.GetSubscriberByGuid(db, jobApplicationDto.Subscriber.SubscriberGuid.Value);
            if (subscriber == null)
            {
                ErrorCode = 404;
                ErrorMsg = $"Subscriber {jobApplicationDto.Subscriber.SubscriberGuid.Value} does not exist";
                return false;
            }
                    
            //validate job posting 
            if (jobApplicationDto.JobPosting == null || jobApplicationDto.JobPosting.JobPostingGuid == null)
            {
                ErrorCode = 400;
                ErrorMsg = "No job posting has been specified";
                return false;
            }
                  
           jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobApplicationDto.JobPosting.JobPostingGuid.Value);

           if (jobPosting == null)
           {
                ErrorCode = 404;
                ErrorMsg = $"Job posting {jobApplicationDto.JobPosting.JobPostingGuid.Value} does not exist";
                return false;
           }
             
 
            if (jobPosting.JobStatus != (int)JobPostingStatus.Active)
            {
                ErrorCode = 400;
                ErrorMsg = $"Job {jobPosting.JobPostingGuid} is not active";
                return false;                
            }

            // verify user has not already applied 
            JobApplication jobApplication = GetJobApplication(db, subscriber.SubscriberId, jobPosting.JobPostingId);
            if ( jobApplication != null )
            {
                ErrorCode = 400;
                ErrorMsg = "User has already applied";
                return false;
            }



            return true;
        }



    }
}
