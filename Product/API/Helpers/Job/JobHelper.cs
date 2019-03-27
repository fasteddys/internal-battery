using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using UpDiddyLib.Helpers;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Factory;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Services;

namespace UpDiddyApi.Helpers.Job
{
    /// <summary>
    /// Helper class for dealing with mapping cc jobpostings to google talent cloud data types
    /// </summary>
    public class JobHelper
    {




        /// <param name="jobPosting"></param>
        /// <returns></returns>
        static public CloudTalentSolution.Job CreateGoogleJob(UpDiddyDbContext db, JobPosting jobPosting)
        {
            // Set default application instructions as required by Google
            CloudTalentSolution.ApplicationInfo applicationInfo = new CloudTalentSolution.ApplicationInfo()
            {
                Instruction = "Apply Now!",
            };

            // Create custom job posting attributes 
            IDictionary<string, CloudTalentSolution.CustomAttribute> customAttributes = CreateJobCustomAttributes(db,jobPosting);

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


        public static void MapCustomJobPostingAttributes(CloudTalentSolution.MatchingJob matchingJob, JobViewDto jobViewDto)
        {
            DateTime dateVal = DateTime.MinValue;
            // Short circuit if the job has no custom attributes 
            if (matchingJob.Job.CustomAttributes == null)
                return;
            // map ApplicationDeadLine 
            if (matchingJob.Job.CustomAttributes["ApplicationDeadlineUTC"] != null )
            {         
                jobViewDto.ApplicationDeadlineUTC = Utils.FromUnixTimeInSeconds(matchingJob.Job.CustomAttributes["ApplicationDeadlineUTC"].LongValues[0].Value);
            }
            // map modify date 
            if (matchingJob.Job.CustomAttributes["ModifyDate"] != null)
            {
                jobViewDto.ModifyDate = Utils.FromUnixTimeInSeconds(matchingJob.Job.CustomAttributes["ModifyDate"].LongValues[0].Value);
            }
            // map posting expiration date 
            if (matchingJob.Job.CustomAttributes["PostingExpirationDateUTC"] != null)
            {
                jobViewDto.PostingExpirationDateUTC = Utils.FromUnixTimeInSeconds(matchingJob.Job.CustomAttributes["PostingExpirationDateUTC"].LongValues[0].Value);
            }
            // map experience level
            if (matchingJob.Job.CustomAttributes["ExperienceLevel"] != null)
            {
                jobViewDto.ExperienceLevel = matchingJob.Job.CustomAttributes["ExperienceLevel"].StringValues[0].ToString();
            }
            // map education level 
            if (matchingJob.Job.CustomAttributes["EducationLevel"] != null)
            {
                jobViewDto.EducationLevel = matchingJob.Job.CustomAttributes["EducationLevel"].StringValues[0].ToString();
            }
            // map employment type 
            if (matchingJob.Job.CustomAttributes["EmploymentType"] != null)
            {
                jobViewDto.EmploymentType = matchingJob.Job.CustomAttributes["EmploymentType"].StringValues[0].ToString();
            }
            // map third party apply url
            if (matchingJob.Job.CustomAttributes["ThirdPartyApplyUrl"] != null)
            {
                jobViewDto.ThirdPartyApplyUrl = matchingJob.Job.CustomAttributes["ThirdPartyApplyUrl"].StringValues[0].ToString();
            }

            // map annual compensation
            if (matchingJob.Job.CustomAttributes["AnnualCompensation"] != null)
            {
                //TODO JAB remove try catch once you remove incorecctly indexed jobs
                try
                {
                    jobViewDto.AnnualCompensation = matchingJob.Job.CustomAttributes["AnnualCompensation"].LongValues[0].Value;
                }
                catch { }
                
            }

            // map telecommute percentage 
            if (matchingJob.Job.CustomAttributes["TelecommutePercentage"] != null)
            {
                jobViewDto.TelecommutePercentage = matchingJob.Job.CustomAttributes["TelecommutePercentage"].LongValues[0].Value;
            }

            // map third party apply flag 
            if (matchingJob.Job.CustomAttributes["ThirdPartyApply"] != null)
            {
                if (matchingJob.Job.CustomAttributes["ThirdPartyApply"].LongValues[0].Value == 1)
                    jobViewDto.ThirdPartyApply = true;
                else
                    jobViewDto.ThirdPartyApply = false;
            }

        }



        /// <summary>
        /// Map cloud talent job results to cc jobsearch results 
        /// </summary>
        /// <param name="searchJobsResponse"></param>
        /// <returns></returns>
        /// TODO JAB finish mapping 
        public static JobSearchResultDto MapSearchResults(ILogger syslog, IMapper mapper, CloudTalentSolution.SearchJobsResponse searchJobsResponse)
        {

            JobSearchResultDto rVal = new JobSearchResultDto();            
            rVal.JobCount = searchJobsResponse.MatchingJobs.Count;
            foreach (CloudTalentSolution.MatchingJob j in searchJobsResponse.MatchingJobs)
            {
                try
                {
                    JobViewDto jv = mapper.Map<JobViewDto>(j);
                    // automapper can't deal with custom job attributes 
                    JobHelper.MapCustomJobPostingAttributes(j, jv);
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
        /// Implements business rules to check the validity of a job posting 
        /// </summary>
        /// <param name="job"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool ValidateJobPosting(JobPosting job, ref string message)
        {
            // TODO JAB move magic strings to constants
            if (job.CompanyId < 0 || job.Company == null)
            {
                message = "Company required";
                return false;
            }

            if (job.SecurityClearance != null && job.SecurityClearanceId == null)
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

        static public bool AddJobToCloudTalent(UpDiddyDbContext db, CloudTalent ct, Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobPostingGuid);
                // validate we have good data 
                if (jobPosting == null || jobPosting.Company == null)
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

        #region Helper functions 
 
 /// <summary>
 ///  Create list of custom attributes for cloud talent job posting 
 /// </summary>
 /// <param name="jobPosting"></param>
 /// <returns></returns>
        static private IDictionary<string, CloudTalentSolution.CustomAttribute> CreateJobCustomAttributes(UpDiddyDbContext db, JobPosting jobPosting)
        {

            IDictionary<string, CloudTalentSolution.CustomAttribute> rVal = new Dictionary<string, CloudTalentSolution.CustomAttribute>();

            // Add application deadline date as a long so it can be queried with boolean <= logic  
            CloudTalentSolution.CustomAttribute ApplicationDeadlineUTC = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { Utils.ToUnixTimeInSeconds(jobPosting.ApplicationDeadlineUTC) }

            };
            rVal.Add("ApplicationDeadlineUTC", ApplicationDeadlineUTC);

            // Add experience level if its been specified 
            if (jobPosting.ExperienceLevel != null)
            {
                CloudTalentSolution.CustomAttribute ExperienceLevel = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.ExperienceLevel.DisplayName }

                };
                rVal.Add("ExperienceLevel", ExperienceLevel);
            }

            // Add education level if its been specified 
            if (jobPosting.EducationLevel != null)
            {
                CloudTalentSolution.CustomAttribute EducationLevel = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.EducationLevel.Level }

                };
                rVal.Add("EducationLevel", EducationLevel);
            }

            // Add compensation normalized to annual values 
            if (jobPosting.Compensation > 0 && jobPosting.CompensationType != null)
            {

                long? AnnualSalary = CompensationTypeFactory.AnnualCompensation(jobPosting.Compensation, jobPosting.CompensationType);
                CloudTalentSolution.CustomAttribute AnnualCompensation = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    LongValues = new List<long?>() { AnnualSalary }
                };
                rVal.Add("AnnualCompensation", AnnualCompensation);
            }

            // Add employment type
            if (jobPosting.EmploymentType != null)
            {
 
                CloudTalentSolution.CustomAttribute EmploymentType = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.EmploymentType.Name }
                };
                rVal.Add("EmploymentType", EmploymentType);
            }

            // Add posting expiration date as a long so it can be queried with boolean <= logic  
            CloudTalentSolution.CustomAttribute PostingExpirationDateUTC = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { Utils.ToUnixTimeInSeconds(jobPosting.PostingExpirationDateUTC) }

            };
            rVal.Add("PostingExpirationDateUTC", PostingExpirationDateUTC);

            // Add posting modify date as a long so it can be queried with boolean <= logic  
            CloudTalentSolution.CustomAttribute ModifyDate = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { Utils.ToUnixTimeInSeconds(jobPosting.ModifyDate.Value) }
            };
            rVal.Add("ModifyDate", ModifyDate);

            // Add posting thirdparty apply as a long so it can be queried with boolean <= logic  
            long ApplyFlag = jobPosting.ThirdPartyApply == true ? 1 : 0;
            CloudTalentSolution.CustomAttribute ThirdPartyApply = new CloudTalentSolution.CustomAttribute()
            {            
                Filterable = true,
                LongValues = new List<long?>() { ApplyFlag }
            };
            rVal.Add("ThirdPartyApply", ThirdPartyApply);

            // Add posting thirdparty url so it can be returned as part of the job        
            CloudTalentSolution.CustomAttribute ThirdPartyApplyUrl = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { jobPosting.ThirdPartyApplicationUrl }
            };
            rVal.Add("ThirdPartyApplyUrl", ThirdPartyApplyUrl);

            // Add telecommute percent as a long so it can be queried with boolean <= logic   
            CloudTalentSolution.CustomAttribute TelecommutePercentage = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { jobPosting.TelecommutePercentage }
            };
            rVal.Add("TelecommutePercentage", TelecommutePercentage);

            var jobSkills = JobPostingFactory.GetPostingSkills(db, jobPosting);
            List<string> skillsList = new List<string>();
            foreach ( JobPostingSkill jps in jobSkills)
            {
                if (jps.Skill != null)
                    skillsList.Add(jps.Skill.SkillName.Trim());
            }
   
            if ( skillsList.Count > 0 )
            {
                CloudTalentSolution.CustomAttribute Skills = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = skillsList
                };
                rVal.Add("Skills", Skills);
            }

            return rVal; 
        }

    }


        #endregion


    
}
