using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using UpDiddyApi.ApplicationCore.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFactory
    {


           
        public static void MapCustomJobPostingAttributes(CloudTalentSolution.MatchingJob matchingJob, JobViewDto jobViewDto )
        {
            DateTime dateVal = DateTime.MinValue;
            // Short circuit if the job has no custom attributes 
            if (matchingJob.Job.CustomAttributes == null)
                return;
            // map ApplicationDeadLine 
            if ( matchingJob.Job.CustomAttributes["ApplicationDeadlineUTC"] != null && 
                 DateTime.TryParse(matchingJob.Job.CustomAttributes["ApplicationDeadlineUTC"].StringValues[0], out dateVal))            
                jobViewDto.ApplicationDeadlineUTC = dateVal;
                            
        }



        /// <summary>
        /// Map cloud talent job results to cc jobsearch results 
        /// </summary>
        /// <param name="searchJobsResponse"></param>
        /// <returns></returns>
        /// TODO JAB finish mapping 
        public static JobSearchResultDto MapSearchResults(ILogger syslog, IMapper mapper, CloudTalentSolution.SearchJobsResponse searchJobsResponse) 
        {
 
            JobSearchResultDto rVal =  new JobSearchResultDto()
            {

            };

            foreach (CloudTalentSolution. MatchingJob j in searchJobsResponse.MatchingJobs)
            { 
                try
                {
                    JobViewDto jv = mapper.Map<JobViewDto>(j);
                    // automapper can't deal with custom job attributes 
                    JobPostingFactory.MapCustomJobPostingAttributes(j, jv);
                    rVal.Jobs.Add(jv);
                }
                catch (Exception e)
                {
                    syslog.LogError(e, "JobPostingFactory.MapSearchResults Error mapping job", e, j);
                }                
            }

            return rVal;
        }


        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>
        /// //TODO JAB make sure all items that need to be indexed into google are included here 
        public static JobPosting GetJobPostingByGuid(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPosting
                .Include(c => c.Company )
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Where(s => s.IsDeleted == 0 && s.JobPostingGuid == guid)
                .FirstOrDefault();
        }


        /// <summary>
        /// Convert to a google cloude talent job object 
        /// </summary>
        /// <param name="jobPosting"></param>
        /// <returns></returns>
        static public CloudTalentSolution.Job ToGoogleJob(JobPosting jobPosting)
        {
            // Set default application instructions as required by Google
            CloudTalentSolution.ApplicationInfo applicationInfo = new CloudTalentSolution.ApplicationInfo()
            {
                Instruction = "Apply Now!",
            };

            // Create custom index attributes container  
            IDictionary<string, CloudTalentSolution.CustomAttribute> customAttributes = new Dictionary<string, CloudTalentSolution.CustomAttribute>();

            /* -------------------  todo add custom attributes as needed 
             * 


            // example of adding custom skills to job which will create a skills facet for search result that can be used to 
            // implement navigators

            CustomAttribute ca = new CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { "Javascript", "C#", "Oracle", ".Net" }
            };
            customAttributes.Add("Skills", ca);

            -------------------------------- */


            // Add application deadline as custom attribute of jobposting.  For constiency use exact JobPosting property name
            // for custom attribute name.  eg.  JobPosting.ApplicationDeadlineUTC -> Google custom attribute named "ApplicationDeadlineUTC"
            CloudTalentSolution.CustomAttribute ApplicationDeadlineUTCAttribute = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { Utils.ISO8601DateString( jobPosting.ApplicationDeadlineUTC) }

            };
            customAttributes.Add("ApplicationDeadlineUTC", ApplicationDeadlineUTCAttribute);

            // Add experience level if its been specified 
            if ( jobPosting.ExperienceLevel != null )
            {
                CloudTalentSolution.CustomAttribute ExperienceLevelAttribute = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.ExperienceLevel.DisplayName }

                };
                customAttributes.Add("ExperienceLevel", ExperienceLevelAttribute);
            }
            
            // Add experience level if its been specified 
            if (jobPosting.EducationLevel != null)
            {
                CloudTalentSolution.CustomAttribute EducationLevelAttribute = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.EducationLevel.Level }

                };
                customAttributes.Add("EducationLevel", EducationLevelAttribute);
            }
            
            // Set the jobs expire timestamp
            string ExpireTimestamp = Utils.GetTimestampAsString(jobPosting.PostingExpirationDateUTC);
 

            var jobToBeCreated = new CloudTalentSolution.Job()
            {
                RequisitionId = jobPosting.JobPostingGuid.ToString(),
                Title = jobPosting.Title,
                Description = jobPosting.Description,
                ApplicationInfo = applicationInfo,
                CompanyName = jobPosting.Company.CloudTalentUri,
                PostingExpireTime = ExpireTimestamp,
                Addresses = new List<string>()
                    {
                       jobPosting.Location
                    } 
            };

            // Add custom attributes if they've been defined 
            if (customAttributes.Count > 0)
                jobToBeCreated.CustomAttributes = customAttributes;

            return jobToBeCreated;
        }

        /// <summary>
        /// Set default properties when a job is being added
        /// </summary>
        /// <param name="job"></param>
        public static void SetDefaultsForAddNew(JobPosting job)
        {

            job.CompanyId = -1;
            job.SecurityClearanceId = null;
            job.IndustryId = null;
            job.EmploymentTypeId = null;
            job.EducationLevelId = null;
            job.ExperienceLevelId = null;

        }


        /// <summary>
        /// Implements business rules to check the validity of a job posting 
        /// </summary>
        /// <param name="job"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool ValidateJobPosting(JobPosting job, ref  string message)
        {
            // TODO JAB move magic strings to constants
           if ( job.CompanyId < 0 || job.Company == null)
           {      
                message = "Company required";
                return false;
           }

           if ( job.SecurityClearance != null && job.SecurityClearanceId == null )
           {
                message = "Invalid security clearance";
                return false;
           }

            if (job.Industry != null && job.IndustryId == null)
            {
                message = "Invalid industry";
                return false;
            }

            if (job.EmploymentType != null && job.EmploymentTypeId == null)
            {
                message = "Invalid employment type";
                return false;
            }

            if (job.EducationLevel != null && job.EducationLevelId == null)
            {
                message = "Invalid education level";
                return false;
            }

            if (job.ExperienceLevel != null && job.ExperienceLevelId == null)
            {
                message = "Invalid experience level";
                return false;
            }

            return true;
        }


        static public bool AddJobToCloudTalent(UpDiddyDbContext db, CloudTalent ct,  Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobPostingGuid);
                // validate we have good data 
                if (jobPosting == null || jobPosting.Company == null )
                    return false;
                // validate the company is known to google, if not add it to the cloud talent 
                if (string.IsNullOrEmpty(jobPosting.Company.CloudTalentUri))
                    ct.IndexCompany(jobPosting.Company);

                // index the job to google 
                CloudTalentSolution.Job job = ct.IndexJob(jobPosting);
                return true;
            }
            catch
            {
                return false;
            }


        }


    }
}
