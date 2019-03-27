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
                .Include(c => c.CompensationType)
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

        }

        public static List<JobPostingSkill> GetPostingSkills(UpDiddyDbContext db, JobPosting jobPosting)
        {
            return db.JobPostingSkill
                .Include(c => c.Skill)
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPosting.JobPostingId)
                .ToList();
        }

        public static void SavePostingSkills(UpDiddyDbContext db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {
            foreach ( SkillDto skillDto in jobPostingDto.Skills)
            {
                JobPostingSkillFactory.Add(db, jobPosting.JobPostingId, skillDto.SkillGuid.Value);
            }
        }

        public static void MapRelatedObjects(UpDiddyDbContext  db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {
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


        }

    }
}
