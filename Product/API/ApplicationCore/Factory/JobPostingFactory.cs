﻿using System;
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
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFactory
    {

        public static string JobPostingUrl(IConfiguration config, Guid subscriberGuid)
        {
            return config["CareerCircle:ViewJobPostingUrl"] + subscriberGuid;
        }

        public static JobPosting GetJobPostingById(UpDiddyDbContext db, int jobPostingId)
        {
            return db.JobPosting
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingId)
                .FirstOrDefault();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static List<JobPosting> GetJobPostingsForSubscriber(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPosting
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == guid)
                .OrderByDescending( s => s.CreateDate)
                .ToList();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static JobPosting GetJobPostingByGuid(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPosting
                .Include(c => c.Company )
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.JobPostingGuid == guid)
                .FirstOrDefault();
        }


 


        /// <summary>
        /// Convert to a google cloude talent job object 
        /// </summary>

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
            job.JobCategoryId = null;

        }

        public static string  GetJobPostingLocation(JobPosting jobPosting)
        {
            return jobPosting.StreetAddress + " " + jobPosting.City + " " + jobPosting.Province + " " + jobPosting.PostalCode;
        }
        public static List<JobPostingSkill> GetPostingSkills(UpDiddyDbContext db, JobPosting jobPosting)
        {
            return db.JobPostingSkill
                .Include(c => c.Skill)
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPosting.JobPostingId)
                .ToList();
        }


        public static void UpdatePostingSkills(UpDiddyDbContext db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {
            JobPostingSkillFactory.DeleteSkillsForPosting(db, jobPosting.JobPostingId);
            foreach (SkillDto skillDto in jobPostingDto.Skills)
            {
                JobPostingSkillFactory.Add(db, jobPosting.JobPostingId, skillDto.SkillGuid.Value);
            }
            db.SaveChanges();
        }


        /// <summary>
        /// Implement business rules to check the validity of an updated job posting 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateUpdatedJobPosting(JobPosting job, IConfiguration config, ref string message)
        {
            if ( string.IsNullOrEmpty(job.CloudTalentUri) )
            {
                message = Constants.JobPosting.ValidationError_JobNotIndexed;
                return false;
            }

            return ValidateJobPosting(job, config, ref message);
        }

        /// <summary>
        /// Implements business rules to check the validity of a job posting 
        /// </summary>
        /// <param name="job"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool ValidateJobPosting(JobPosting job, IConfiguration config, ref string message)
        {

            int MinDescriptionLen = int.Parse(config["CloudTalent:JobDescriptionMinLength"]);
            
            if ( job.Description.Trim().Length < MinDescriptionLen)
            {
                message = string.Format(Constants.JobPosting.ValidationError_InvalidDescriptionLength, MinDescriptionLen);
                return false;
            }


            if (job.Subscriber == null && job.SubscriberId == null)
            {
                message = Constants.JobPosting.ValidationError_SubscriberRequiredMsg;
                return false;
            }

            if (job.CompanyId < 0 || job.Company == null)
            {

                message = Constants.JobPosting.ValidationError_CompanyRequiredMsg;
                return false;
            }

            if (job.SecurityClearance != null && job.SecurityClearanceId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidSecurityClearanceMsg;
                return false;
            }

            if (job.Industry != null && job.IndustryId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidIndustryMsg;
                return false;
            }

            if (job.JobCategory != null && job.JobCategoryId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidJobCategoryMsg;
                return false;
            }

            if (job.EmploymentType != null && job.EmploymentTypeId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidEmploymentTypeMsg;
                return false;
            }

            if (job.EducationLevel != null && job.EducationLevelId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidEducationLevelMsg;
                return false;
            }

            if (job.ExperienceLevel != null && job.ExperienceLevelId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidExperienceLevelMsg;
                return false;
            }

            return true;
        }

        public static void CopyPostingSkills(UpDiddyDbContext db, int sourcePostingId, int destinationPostingId)
        {
            List<JobPostingSkill> skills = JobPostingSkillFactory.GetSkillsForPosting(db, sourcePostingId);
            foreach ( JobPostingSkill s in skills)
            {
                JobPostingSkillFactory.Add(db, destinationPostingId, s.Skill.SkillGuid.Value);
            }
            db.SaveChanges();
        }




        /// <summary>
        /// Save posting skills - todo use stored procedure to make this more efficient 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jobPosting"></param>
        /// <param name="jobPostingDto"></param>
        public static void SavePostingSkills(UpDiddyDbContext db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {

            if (jobPostingDto.Skills == null)
                return;

            foreach ( SkillDto skillDto in jobPostingDto.Skills)
            {
                JobPostingSkillFactory.Add(db, jobPosting.JobPostingId, skillDto.SkillGuid.Value);
            }
            db.SaveChanges();
        }
        /// <summary>
        /// Wire up the integer ids of all the navigation objects.   
        /// todo - find a better way to do this since it's highly unefficient.  Some options included a) exposing the dumb key via dtos 
        /// to the front end so they can pass it back and eliminate this step b) research EF to see if we can use navigation properties on GUIDS 
        /// rather than dumb int ids c) use a stored procedure to make this more efficient
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jobPosting"></param>
        /// <param name="jobPostingDto"></param>
        public static void MapRelatedObjects(UpDiddyDbContext  db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {

            // map subscriber 
            if (jobPostingDto.Subscriber != null)
            {
                Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(db, jobPostingDto.Subscriber.SubscriberGuid.Value);
                if (subscriber != null)
                    jobPosting.SubscriberId = subscriber.SubscriberId;
            }

            // map company id 
            if (jobPostingDto.Company != null)
            {
                Company company = CompanyFactory.GetCompanyByGuid(db, jobPostingDto.Company.CompanyGuid);
                if (company != null)
                    jobPosting.CompanyId = company.CompanyId;
            }
            // map industry id
            if (jobPostingDto.Industry != null)
            {
                Industry industry = IndustryFactory.GetIndustryByGuid(db, jobPostingDto.Industry.IndustryGuid);
                if (industry != null)
                    jobPosting.IndustryId = industry.IndustryId;
            }
            // map security clearance 
            if (jobPostingDto.SecurityClearance != null)
            {
                SecurityClearance securityClearance = SecurityClearanceFactory.GetSecurityClearanceByGuid(db, jobPostingDto.SecurityClearance.SecurityClearanceGuid);
                if (securityClearance != null)
                    jobPosting.SecurityClearanceId = securityClearance.SecurityClearanceId;
            }
            // map employment type
            if (jobPostingDto.EmploymentType != null)
            {
                EmploymentType employmentType = EmploymentTypeFactory.GetEmploymentTypeByGuid(db, jobPostingDto.EmploymentType.EmploymentTypeGuid);
                if (employmentType != null)
                    jobPosting.EmploymentTypeId = employmentType.EmploymentTypeId;
            }
            // map educational level type
            if (jobPostingDto.EducationLevel != null)
            {
                EducationLevel educationLevel = EducationLevelFactory.GetEducationLevelByGuid(db, jobPostingDto.EducationLevel.EducationLevelGuid);
                if (educationLevel != null)
                    jobPosting.EducationLevelId = educationLevel.EducationLevelId;
            }
            // map level experience type
            if (jobPostingDto.ExperienceLevel != null)
            {
                ExperienceLevel experienceLevel = ExperienceLevelFactory.GetExperienceLevelByGuid(db, jobPostingDto.ExperienceLevel.ExperienceLevelGuid);
                if (experienceLevel != null)
                    jobPosting.ExperienceLevelId = experienceLevel.ExperienceLevelId;
            }
            // map job category
            if (jobPostingDto.JobCategory != null)
            {
                JobCategory jobCategory = JobCategoryFactory.GetJobCategoryByGuid(db, jobPostingDto.JobCategory.JobCategoryGuid);
                if (jobCategory != null)
                    jobPosting.JobCategoryId = jobCategory.JobCategoryId;
            }

            // map compensation type 
            if (jobPostingDto.CompensationType != null)
            {
                CompensationType compensationType = CompensationTypeFactory.GetCompensationTypeByGuid(db, jobPostingDto.CompensationType.CompensationTypeGuid);
                if (compensationType != null)
                    jobPosting.CompensationTypeId = compensationType.CompensationTypeId;
            }
      


        }



        public static void MapRelatedObjects(JobPosting jobPosting, JobPosting sourceJobPosting)
        {
            // map company id 
            if (sourceJobPosting.Company != null)
                jobPosting.CompanyId = sourceJobPosting.Company.CompanyId;
            // map industry id
            if (sourceJobPosting.Industry != null)  
                    jobPosting.IndustryId = sourceJobPosting.Industry.IndustryId;            
            // map security clearance 
            if (sourceJobPosting.SecurityClearance != null)
                jobPosting.SecurityClearanceId = sourceJobPosting.SecurityClearance.SecurityClearanceId;
    
            // map employment type
            if (sourceJobPosting.EmploymentType != null)
                jobPosting.EmploymentTypeId = sourceJobPosting.EmploymentType.EmploymentTypeId;
            // map educational level type
            if (sourceJobPosting.EducationLevel != null)
                jobPosting.EducationLevelId = sourceJobPosting.EducationLevel.EducationLevelId;
            // map level experience type
            if (sourceJobPosting.ExperienceLevel != null)
                jobPosting.ExperienceLevelId = sourceJobPosting.ExperienceLevel.ExperienceLevelId;
            // map job category
            if (sourceJobPosting.JobCategory != null)
                jobPosting.JobCategoryId = sourceJobPosting.JobCategory.JobCategoryId;
            // map compensation type 
            if (sourceJobPosting.CompensationType != null)
                jobPosting.CompensationTypeId = sourceJobPosting.CompensationType.CompensationTypeId;

        }


    }
}
