using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore
{
    public class JobApplicationFactory
    {


        public static async Task <List<JobApplication>> GetJobApplicationsForSubscriber(IRepositoryWrapper repositoryWrapper, int subscriberId)
        {
            return await repositoryWrapper.JobApplication.GetAll()
                .Include( s => s.JobPosting)
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .ToListAsync();
        }


        public static async Task<List<JobApplication>> GetJobApplicationsForPosting(IRepositoryWrapper repositoryWrapper, int jobPostingID)
        {
            return await repositoryWrapper.JobApplication.GetAll()             
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingID)       
                .ToListAsync();
        }


        public static async Task<JobApplication> GetJobApplicationByGuid(IRepositoryWrapper repositoryWrapper, Guid jobApplicationGuid)
        {
            return await repositoryWrapper.JobApplication.GetAll()
                .Include(s => s.JobPosting)
                .Include(s => s.Subscriber)
                .Where(s => s.IsDeleted == 0  && s.JobApplicationGuid == jobApplicationGuid)                   
                .FirstOrDefaultAsync();
        }


        public static async Task<JobApplication> GetJobApplication(IRepositoryWrapper repositoryWrapper, int subscriberId, int jobPostingID)
        {
            return await repositoryWrapper.JobApplication.GetAll()
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingID && s.SubscriberId == subscriberId)
                .FirstOrDefaultAsync();
        }

        static public bool ValidateJobApplication(IRepositoryWrapper repositoryWrapper, JobApplicationDto jobApplicationDto, ref Subscriber subscriber, ref JobPosting jobPosting, ref int ErrorCode, ref string ErrorMsg)
        {

            if (jobApplicationDto == null )
            {
                ErrorCode = 400;
                ErrorMsg = "Job application is required.";
                return false;
            }



            //validate subscriber 
            if (jobApplicationDto.Subscriber == null || jobApplicationDto.Subscriber.SubscriberGuid == null)
            {
                ErrorCode = 400;
                ErrorMsg = "No subscriber has been specified.";
                return false;
            }
                        
            subscriber = SubscriberFactory.GetSubscriberWithSubscriberFiles(repositoryWrapper, jobApplicationDto.Subscriber.SubscriberGuid.Value).Result;
            if (subscriber == null)
            {
                ErrorCode = 404;
                ErrorMsg = $"Subscriber {jobApplicationDto.Subscriber.SubscriberGuid.Value} does not exist.";
                return false;
            }

            //validate that resume and cover letter are present
            if(subscriber.SubscriberFile.FirstOrDefault() == null || string.IsNullOrEmpty(jobApplicationDto.CoverLetter))
            {
                ErrorCode = 400;
                ErrorMsg = "Subscriber has not supplied both a cover letter and resume.";
                return false;
            }
                    
            //validate job posting 
            if (jobApplicationDto.JobPosting == null || jobApplicationDto.JobPosting.JobPostingGuid == null)
            {
                ErrorCode = 400;
                ErrorMsg = "No job posting has been specified.";
                return false;
            }
                  
           jobPosting = JobPostingFactory.GetJobPostingByGuid(repositoryWrapper, jobApplicationDto.JobPosting.JobPostingGuid.Value).Result;

           if (jobPosting == null)
           {
                ErrorCode = 404;
                ErrorMsg = $"Job posting {jobApplicationDto.JobPosting.JobPostingGuid.Value} does not exist.";
                return false;
           }
             
 
            if (jobPosting.JobStatus != (int)JobPostingStatus.Active)
            {
                ErrorCode = 400;
                ErrorMsg = $"Job {jobPosting.JobPostingGuid} is not active.";
                return false;                
            }

            // verify user has not already applied 
            JobApplication jobApplication = GetJobApplication(repositoryWrapper, subscriber.SubscriberId, jobPosting.JobPostingId).Result;
            if ( jobApplication != null )
            {
                ErrorCode = 400;
                ErrorMsg = "User has already applied.";
                return false;
            }



            return true;
        }



    }
}
