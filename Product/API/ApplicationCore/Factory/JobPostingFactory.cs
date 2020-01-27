using System;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Interfaces;
using System.Data;
using System.Data.SqlClient;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFactory
    {
        /// <summary>
        /// This method replaces the Update, Copy, and Save methods which are now marked as obsolete in JobPostingFactory
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jobPostingId"></param>
        /// <param name="jobPostingDto"></param>
        public static void UpdateJobPostingSkills(IRepositoryWrapper repositoryWrapper, int jobPostingId, List<SkillDto> jobPostingSkills)
        {
            var jobPostingIdParam = new SqlParameter("@JobPostingId", jobPostingId);

            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            if (jobPostingSkills != null)
            {
                foreach (var skill in jobPostingSkills)
                {
                    table.Rows.Add(skill.SkillGuid);
                }
            }

            var skillGuids = new SqlParameter("@SkillGuids", table);
            skillGuids.SqlDbType = SqlDbType.Structured;
            skillGuids.TypeName = "dbo.GuidList";

            var spParams = new object[] { jobPostingIdParam, skillGuids };

            var rowsAffected = repositoryWrapper.JobPostingSkillRepository.ExecuteSQL(@"
                EXEC [dbo].[System_Update_JobPostingSkills] 
                    @JobPostingId,
	                @SkillGuids", spParams);
        }

        public static void SetMetaData(JobPosting jobPosting, JobPostingDto jobPostingDto)
        {
            // add meta data for seo 
            jobPostingDto.MetaDescription = $"Search for {jobPostingDto.Title} jobs near {jobPostingDto.CityProvince} with CareerCircle and find your next great opportunity today.";
            jobPostingDto.MetaTitle = jobPosting.Title;
            int numTerms = 3;
            jobPostingDto.MetaKeywords = jobPostingDto.CityProvince.Replace(',', ' ') + " Employment, Work, " + jobPostingDto.Title + " Jobs ";
            if (jobPostingDto.Industry != null)
            {
                jobPostingDto.MetaKeywords += ", " + jobPostingDto.Industry.Name + " Jobs ";

            }

            if (jobPostingDto.JobCategory != null)
            {
                jobPostingDto.MetaKeywords += ", " + jobPostingDto.JobCategory.Name + " Jobs ";
                ++numTerms;
            }

            // per foley - limit to 10 terms at max
            foreach (SkillDto s in jobPostingDto.JobPostingSkills)
            {
                jobPostingDto.MetaKeywords += ", " + s.SkillName + " Jobs";
                ++numTerms;
                // magic number ok since this is an industry standard that's not going to change 
                if (numTerms == 10)
                    break;
            }
        }

        public static string JobPostingFullyQualifiedUrl(IConfiguration config, JobPostingDto jobPostingDto)
        {


            string jobPostingUrl = config["Environment:BaseUrl"].TrimEnd('/') + Utils.CreateSemanticJobPath(
                 jobPostingDto.Industry == null ? string.Empty : jobPostingDto.Industry.Name,
                 jobPostingDto.JobCategory == null ? string.Empty : jobPostingDto.JobCategory.Name,
                 jobPostingDto.Country == null ? string.Empty : jobPostingDto.Country,
                 jobPostingDto.Province == null ? string.Empty : jobPostingDto.Province,
                 jobPostingDto.City == null ? string.Empty : jobPostingDto.City,
                 jobPostingDto.JobPostingGuid.ToString()
                );
            return jobPostingUrl;
        }

        public static async Task<List<JobPosting>> GetAllJobPostingsForSitemap(IRepositoryWrapper repositoryWrapper)
        {
            // note that this doesn't include all related entities; only those that we need to build the semantic url
            return await repositoryWrapper.JobPosting.GetAllWithTracking()
                .Include(jp => jp.Industry)
                .Include(jp => jp.JobCategory)
                .Where(s => s.IsDeleted == 0)
                .Select(jp => new JobPosting()
                {
                    JobPostingGuid = jp.JobPostingGuid,
                    Industry = new Industry() { Name = jp.Industry.Name },
                    JobCategory = new JobCategory() { Name = jp.JobCategory.Name },
                    Country = jp.Country,
                    Province = jp.Province,
                    City = jp.City,
                    ModifyDate = jp.ModifyDate.HasValue ? jp.ModifyDate.Value : jp.CreateDate
                })
                .ToListAsync();
        }

        public static bool DeleteJob(IRepositoryWrapper repositoryWrapper, Guid jobPostingGuid, ref string ErrorMsg, ILogger syslog, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, IHangfireService _hangfireService)
        {

            JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(repositoryWrapper, jobPostingGuid).Result;
            if (jobPosting == null)
            {
                ErrorMsg = $"Job posting {jobPostingGuid} does not exist";
                return false;
            }
            Recruiter recruiter = RecruiterFactory.GetRecruiterById(repositoryWrapper, jobPosting.RecruiterId.Value).Result;
            if (recruiter == null)
            {
                ErrorMsg = $"Recruiter {jobPosting.RecruiterId.Value} rec not found";
                return false;
            }

            if (jobPosting.RecruiterId != recruiter.RecruiterId)
            {
                ErrorMsg = "JobPosting owner is not specified or does not match user posting job";
                return false;
            }

            // queue a job to delete the posting from the job index and mark it as deleted in sql server
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteJob(jobPosting.JobPostingGuid));
            syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
            return true;
        }

        public static bool PostJob(IRepositoryWrapper repositoryWrapper, int recruiterId, JobPostingDto jobPostingDto, ref Guid newPostingGuid, ref string ErrorMsg, ILogger syslog, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, bool isAcceptsNewSkills, IHangfireService _hangfireService)
        {
            if (isAcceptsNewSkills && jobPostingDto?.JobPostingSkills != null)
            {
                var updatedSkills = new List<SkillDto>();
                foreach (var skillDto in jobPostingDto.JobPostingSkills)
                {
                    var skill = SkillFactory.GetOrAdd(repositoryWrapper, skillDto.SkillName).Result;
                    if (!updatedSkills.Exists(s => s.SkillGuid == skill.SkillGuid))
                        updatedSkills.Add(new SkillDto()
                        {
                            SkillGuid = skill.SkillGuid,
                            SkillName = skill.SkillName
                        });
                }
                jobPostingDto.JobPostingSkills = updatedSkills;
            }

            return PostJob(repositoryWrapper, recruiterId, jobPostingDto, ref newPostingGuid, ref ErrorMsg, syslog, mapper, configuration, _hangfireService);
        }

        public static bool PostJob(IRepositoryWrapper repositoryWrapper, int recruiterId, JobPostingDto jobPostingDto, ref Guid newPostingGuid, ref string ErrorMsg, ILogger syslog, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, IHangfireService _hangfireService)
        {
            int postingTTL = int.Parse(configuration["JobPosting:PostingTTLInDays"]);

            if (jobPostingDto == null)
            {
                ErrorMsg = "JobPosting is required";
                return false;
            }

            syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");

            JobPosting jobPosting = mapper.Map<JobPosting>(jobPostingDto);
            // todo find a better way to deal with the job posting having a collection of JobPostingSkill and the job posting DTO having a collection of SkillDto
            // ignore posting skills that were mapped via automapper, they will be associated with the posting below 
            jobPosting.JobPostingSkills = null;
            // assign recruiter
            jobPosting.RecruiterId = recruiterId;
            // use factory method to make sure all the base data values are set just 
            // in case the caller didn't set them
            BaseModelFactory.SetDefaultsForAddNew(jobPosting);
            // important! Init all reference object ids to null since further logic will use < 0 to check for 
            // their validity
            JobPostingFactory.SetDefaultsForAddNew(jobPosting);
            // Asscociate related objects that were passed by guid
            // todo find a more efficient way to do this
            JobPostingFactory.MapRelatedObjects(repositoryWrapper, jobPosting, jobPostingDto).Wait();

            string msg = string.Empty;

            if (JobPostingFactory.ValidateJobPosting(jobPosting, configuration, ref msg) == false)
            {
                ErrorMsg = msg;
                syslog.Log(LogLevel.Warning, "JobPostingController.CreateJobPosting:: Bad Request {Description} {JobPosting}", msg, jobPostingDto);
                return false;
            }

            jobPosting.CloudTalentIndexStatus = (int)GoogleCloudIndexStatus.NotIndexed;
            jobPosting.JobPostingGuid = Guid.NewGuid();
            // set expiration date 
            if (jobPosting.PostingDateUTC < DateTime.UtcNow)
                jobPosting.PostingDateUTC = DateTime.UtcNow;
            if (jobPosting.PostingExpirationDateUTC < DateTime.UtcNow)
            {
                jobPosting.PostingExpirationDateUTC = DateTime.UtcNow.AddDays(postingTTL);
            }

            // adding this try/catch to troubleshoot an issue that i am unable to replicate in my local environment
            try
            {
                // save the job to sql server 
                // todo make saving the job posting and skills more efficient with a stored procedure 
                repositoryWrapper.JobPosting.Create(jobPosting);
                repositoryWrapper.JobPosting.SaveAsync().Wait();
                // update associated job posting skills
                JobPostingFactory.UpdateJobPostingSkills(repositoryWrapper, jobPosting.JobPostingId, jobPostingDto?.JobPostingSkills);
                //index active jobs into google 
                if (jobPosting.JobStatus == (int)JobPostingStatus.Active)
                    _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(jobPosting.JobPostingGuid));


                newPostingGuid = jobPosting.JobPostingGuid;
            }
            catch (AggregateException ae)
            {
                syslog.Log(LogLevel.Information, $"***** JobPostingFactory:PostJob aggregate exception : {ae.Message}, Source: {ae.Source}, StackTrace: {ae.StackTrace}");

                foreach (var e in ae.InnerExceptions)
                {
                    if (e.InnerException != null)
                    {
                        syslog.Log(LogLevel.Information, $"***** JobPostingFactory:PostJob aggregate exception inner exception instance: {e.InnerException.Message}, Source: {e.InnerException.Source}, StackTrace: {e.InnerException.StackTrace}");
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Information, $"***** JobPostingFactory:PostJob generic exception: {e.Message}, Source: {e.Source}, StackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                    syslog.Log(LogLevel.Information, $"***** JobPostingFactory:PostJob generic inner exception: {e.InnerException.Message}, Source: {e.InnerException.Source}, StackTrace: {e.InnerException.StackTrace}");
            }
            finally
            {
                syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
            }

            return true;
        }



        public static async Task<JobPosting> GetJobPostingById(IRepositoryWrapper repositoryWrapper, int jobPostingId)
        {
            return await repositoryWrapper.JobPosting.GetAllWithTracking()
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Recruiter.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingId)
                .FirstOrDefaultAsync();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static async Task<List<JobPosting>> GetJobPostingsForSubscriber(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
 
            return await repositoryWrapper.JobPosting.GetAllWithTracking()
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Recruiter.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.Recruiter.Subscriber.SubscriberGuid == guid)
                .OrderByDescending(s => s.CreateDate)
                .ToListAsync();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static async Task<JobPosting> GetJobPostingByGuidWithRelatedObjects(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
            return await repositoryWrapper.JobPosting.GetAllWithTracking()
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Recruiter.Subscriber)
                .Include(c => c.JobPostingSkills).ThenInclude(ss => ss.Skill)
                .Where(s => s.IsDeleted == 0 && s.JobPostingGuid == guid)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static async Task<JobPosting> GetJobPostingByGuidWithRelatedObjectsAsync(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
            return await repositoryWrapper.JobPosting.GetAllWithTracking()
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Recruiter.Subscriber)
                .Include(c => c.JobPostingSkills).ThenInclude(ss => ss.Skill)
                .Where(s => s.JobPostingGuid == guid)
                .FirstOrDefaultAsync();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static async Task<JobPosting> GetJobPostingByGuid(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
            return await repositoryWrapper.JobPosting.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.JobPostingGuid == guid)
                .Include(s => s.Recruiter).ThenInclude(r => r.Subscriber)
                .Include(s => s.Company)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get an expired job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static async Task<JobPosting> GetExpiredJobPostingByGuid(IRepositoryWrapper repositoryWrapper, Guid guid)
        {
            return await repositoryWrapper.JobPosting.GetAllWithTracking()
                .Where(s => s.IsDeleted == 1 && s.JobPostingGuid == guid)
                .FirstOrDefaultAsync();
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

        public static string GetJobPostingLocation(JobPosting jobPosting)
        {
            return jobPosting.StreetAddress + " " + jobPosting.City + " " + jobPosting.Province + " " + jobPosting.PostalCode;
        }
        public static async Task<List<JobPostingSkill>> GetPostingSkills(IRepositoryWrapper repositoryWrapper, JobPosting jobPosting)
        {
            return await repositoryWrapper.JobPostingSkillRepository.GetAllWithTracking()
               .Include(c => c.Skill)
               .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPosting.JobPostingId)
               .ToListAsync();
        }

        [Obsolete("This method of modifying job skills is slow and should not be used.", true)]
        public static async Task UpdatePostingSkills(IRepositoryWrapper repositoryWrapper, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {
            await JobPostingSkillFactory.DeleteSkillsForPosting(repositoryWrapper, jobPosting.JobPostingId);
            if (jobPostingDto.JobPostingSkills == null)
                return;
            foreach (SkillDto skillDto in jobPostingDto.JobPostingSkills)
            {
                await JobPostingSkillFactory.Add(repositoryWrapper, jobPosting.JobPostingId, skillDto.SkillGuid.Value);
            }
            await repositoryWrapper.JobPostingSkillRepository.SaveAsync();
        }


        /// <summary>
        /// Implement business rules to check the validity of an updated job posting 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateUpdatedJobPosting(JobPosting job, IConfiguration config, ref string message)
        {

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

            if (job.Description.Trim().Length < MinDescriptionLen)
            {
                message = string.Format(Constants.JobPosting.ValidationError_InvalidDescriptionLength, MinDescriptionLen);
                return false;
            }


            if ( job.ThirdPartyApply == false && job.Recruiter.Subscriber == null && job.Recruiter.SubscriberId == null)
            {
                message = Constants.JobPosting.ValidationError_SubscriberRequiredMsg;
                return false;
            }

            if (job.CompanyId < 0 || job.Company == null)
            {

                message = Constants.JobPosting.ValidationError_CompanyRequiredMsg;
                return false;
            }

            if ( job.SecurityClearance != null && job.SecurityClearanceId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidSecurityClearanceMsg;
                return false;
            }

            if ( job.Industry != null && job.IndustryId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidIndustryMsg;
                return false;
            }

            if ( job.JobCategory != null && job.JobCategoryId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidJobCategoryMsg;
                return false;
            }

            if ( job.EmploymentType != null && job.EmploymentTypeId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidEmploymentTypeMsg;
                return false;
            }

            if ( job.EducationLevel != null && job.EducationLevelId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidEducationLevelMsg;
                return false;
            }

            if ( job.ExperienceLevel != null && job.ExperienceLevelId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidExperienceLevelMsg;
                return false;
            }

            return true;
        }

        [Obsolete("This method of modifying job skills is slow and should not be used.", true)]
        public static async Task CopyPostingSkills(IRepositoryWrapper repositoryWrapper, int sourcePostingId, int destinationPostingId)
        {
            List<JobPostingSkill> skills = await JobPostingSkillFactory.GetSkillsForPosting(repositoryWrapper, sourcePostingId);
            foreach (JobPostingSkill s in skills)
            {
                await JobPostingSkillFactory.Add(repositoryWrapper, destinationPostingId, s.Skill.SkillGuid.Value);
            }
            await repositoryWrapper.JobPostingSkillRepository.SaveAsync();
        }




        /// <summary>
        /// Save posting skills - todo use stored procedure to make this more efficient 
        /// </summary>
        /// <param name="repositoryWrapper"></param>
        /// <param name="jobPosting"></param>
        /// <param name="jobPostingDto"></param>
        /// <returns></returns>
        [Obsolete("This method of modifying job skills is slow and should not be used.", true)]
        public static async Task SavePostingSkills(IRepositoryWrapper repositoryWrapper, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {

            if (jobPostingDto.JobPostingSkills == null)
                return;

            foreach (SkillDto skillDto in jobPostingDto.JobPostingSkills)
            {
                await JobPostingSkillFactory.Add(repositoryWrapper, jobPosting.JobPostingId, skillDto.SkillGuid.Value);
            }
            await repositoryWrapper.JobPostingSkillRepository.SaveAsync();
        }
        /// <summary>
        /// Wire up the integer ids of all the navigation objects.   
        /// todo - find a better way to do this since it's highly unefficient.  Some options included a) exposing the dumb key via dtos 
        /// to the front end so they can pass it back and eliminate this step b) research EF to see if we can use navigation properties on GUIDS 
        /// rather than dumb int ids c) use a stored procedure to make this more efficient
        /// </summary>
        /// <param name="repositoryWrapper"></param>
        /// <param name="jobPosting"></param>
        /// <param name="jobPostingDto"></param>
        public static async Task MapRelatedObjects(IRepositoryWrapper repositoryWrapper, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {

            // map subscriber 
            if (jobPostingDto.Recruiter.Subscriber != null)
            {
                Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(repositoryWrapper, jobPostingDto.Recruiter.Subscriber.SubscriberGuid.Value);
                if (subscriber != null)
                    jobPosting.Recruiter.SubscriberId = subscriber.SubscriberId;
            }

            // map company id 
            if (jobPostingDto.Company != null)
            {
                Company company = await CompanyFactory.GetCompanyByGuid(repositoryWrapper, jobPostingDto.Company.CompanyGuid);
                if (company != null)
                    jobPosting.CompanyId = company.CompanyId;
            }
            // map industry id
            if (jobPostingDto.Industry != null)
            {
                Industry industry = await IndustryFactory.GetIndustryByGuid(repositoryWrapper, jobPostingDto.Industry.IndustryGuid);
                if (industry != null)
                    jobPosting.IndustryId = industry.IndustryId;
            }
            // map security clearance 
            if (jobPostingDto.SecurityClearance != null)
            {
                SecurityClearance securityClearance = await SecurityClearanceFactory.GetSecurityClearanceByGuid(repositoryWrapper, jobPostingDto.SecurityClearance.SecurityClearanceGuid);
                if (securityClearance != null)
                    jobPosting.SecurityClearanceId = securityClearance.SecurityClearanceId;
            }
            // map employment type
            if (jobPostingDto.EmploymentType != null)
            {
                EmploymentType employmentType = await EmploymentTypeFactory.GetEmploymentTypeByGuid(repositoryWrapper, jobPostingDto.EmploymentType.EmploymentTypeGuid);
                if (employmentType != null)
                    jobPosting.EmploymentTypeId = employmentType.EmploymentTypeId;
            }
            // map educational level type
            if (jobPostingDto.EducationLevel != null)
            {
                EducationLevel educationLevel = await EducationLevelFactory.GetEducationLevelByGuid(repositoryWrapper, jobPostingDto.EducationLevel.EducationLevelGuid);
                if (educationLevel != null)
                    jobPosting.EducationLevelId = educationLevel.EducationLevelId;
            }
            // map level experience type
            if (jobPostingDto.ExperienceLevel != null)
            {
                ExperienceLevel experienceLevel = await ExperienceLevelFactory.GetExperienceLevelByGuid(repositoryWrapper, jobPostingDto.ExperienceLevel.ExperienceLevelGuid);
                if (experienceLevel != null)
                    jobPosting.ExperienceLevelId = experienceLevel.ExperienceLevelId;
            }
            // map job category
            if (jobPostingDto.JobCategory != null)
            {
                JobCategory jobCategory = await JobCategoryFactory.GetJobCategoryByGuid(repositoryWrapper, jobPostingDto.JobCategory.JobCategoryGuid);
                if (jobCategory != null)
                    jobPosting.JobCategoryId = jobCategory.JobCategoryId;
            }

            // map compensation type 
            if (jobPostingDto.CompensationType != null)
            {
                CompensationType compensationType = await CompensationTypeFactory.GetCompensationTypeByGuid(repositoryWrapper, jobPostingDto.CompensationType.CompensationTypeGuid);
                if (compensationType != null)
                    jobPosting.CompensationTypeId = compensationType.CompensationTypeId;
            }



        }

        public static async Task<JobPosting> CopyJobPosting(IRepositoryWrapper repositoryWrapper, JobPosting jobPosting, int postingTTL)
        {
            repositoryWrapper.JobPosting.GetEntry(jobPosting).State = EntityState.Detached;
            // use factory method to make sure all the base data values are set just 
            // in case the caller didn't set them
            BaseModelFactory.SetDefaultsForAddNew(jobPosting);
            // important! Init all reference object ids to null since further logic will use < 0 to check for 
            // their validity
            JobPostingFactory.SetDefaultsForAddNew(jobPosting);
            // assign new guid 
            jobPosting.JobPostingGuid = Guid.NewGuid();
            // null out identity column
            int SourcePostingId = jobPosting.JobPostingId;
            jobPosting.JobPostingId = 0;
            // null out skills = they will be handled explicity below
            jobPosting.JobPostingSkills = null;
            // set google index statuses 
            jobPosting.CloudTalentIndexInfo = string.Empty;
            jobPosting.CloudTalentIndexStatus = (int)GoogleCloudIndexStatus.NotIndexed;
            jobPosting.CloudTalentUri = string.Empty;
            // set posting as draft 
            jobPosting.JobStatus = (int)JobPostingStatus.Draft;
            // Set title 
            jobPosting.Title += " (copy)";
            // set expiration date 
            if (jobPosting.PostingDateUTC < DateTime.UtcNow)
                jobPosting.PostingDateUTC = DateTime.UtcNow;
            if (jobPosting.PostingExpirationDateUTC < DateTime.UtcNow)
            {
                jobPosting.PostingExpirationDateUTC = DateTime.UtcNow.AddDays(postingTTL);
            }

            await repositoryWrapper.JobPosting.Create(jobPosting);
            await repositoryWrapper.JobPosting.SaveAsync();
            // get existing skills from job posting and add them to the new job posting
            var jobPostingSkills = jobPosting.JobPostingSkills.Select(s => new SkillDto() { SkillGuid = s.Skill.SkillGuid }).ToList();
            JobPostingFactory.UpdateJobPostingSkills(repositoryWrapper, jobPosting.JobPostingId, jobPostingSkills);

            return jobPosting;
        }

        public static bool UpdateJobPosting(IRepositoryWrapper repositoryWrapper, Guid jobPostingGuid, JobPostingDto jobPostingDto, ref string ErrorMsg, bool isAcceptsNewSkills, IHangfireService _hangfireService)
        {
            if (isAcceptsNewSkills && jobPostingDto?.JobPostingSkills != null)
            {
                var updatedSkills = new List<SkillDto>();
                foreach (var skillDto in jobPostingDto.JobPostingSkills)
                {
                    var skill = SkillFactory.GetOrAdd(repositoryWrapper, skillDto.SkillName).Result;
                    if (!updatedSkills.Exists(s => s.SkillGuid == skill.SkillGuid))
                        updatedSkills.Add(new SkillDto()
                        {
                            SkillGuid = skill.SkillGuid,
                            SkillName = skill.SkillName
                        });
                }
                jobPostingDto.JobPostingSkills = updatedSkills;
            }

            return UpdateJobPosting(repositoryWrapper, jobPostingGuid, jobPostingDto, ref ErrorMsg, _hangfireService);
        }

        public static bool UpdateJobPosting(IRepositoryWrapper repositoryWrapper, Guid jobPostingGuid, JobPostingDto jobPostingDto, ref string ErrorMsg, IHangfireService _hangfireService)
        {

            try
            {
                // Retreive the current state of the job posting 
                JobPosting jobPosting = null;

                // for backward compatability, try and find the posting by the value specified in the jobposting DTO, if not it's not specified try and find int based on the passed job posting guid
                if (jobPostingDto.JobPostingGuid != null)
                    jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(repositoryWrapper, jobPostingDto.JobPostingGuid.Value).Result;
                else
                    jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(repositoryWrapper, jobPostingGuid).Result;

                if (jobPosting == null)
                {
                    ErrorMsg = $"{jobPostingDto.JobPostingGuid} is not a valid jobposting guid";
                    return false;
                }

                if (jobPosting.ThirdPartyApply)
                {
                    // update the recruiter information if it has changed since the last time the job page was inspected
                    RecruiterFactory.GetAddOrUpdate(repositoryWrapper, jobPostingDto.Recruiter.Email, jobPostingDto.Recruiter.FirstName, jobPostingDto.Recruiter.LastName, jobPostingDto.Recruiter.PhoneNumber, null).Wait();
                }

                jobPosting.Title = jobPostingDto.Title;
                jobPosting.Description = jobPostingDto.Description;
                jobPosting.PostingExpirationDateUTC = jobPostingDto.PostingExpirationDateUTC;
                jobPosting.ApplicationDeadlineUTC = jobPostingDto.ApplicationDeadlineUTC;
                jobPosting.JobStatus = jobPostingDto.JobStatus;
                jobPosting.IsAgencyJobPosting = jobPostingDto.IsAgencyJobPosting;
                jobPosting.H2Visa = jobPostingDto.H2Visa;
                jobPosting.TelecommutePercentage = jobPostingDto.TelecommutePercentage;
                jobPosting.Compensation = jobPostingDto.Compensation;
                jobPosting.ThirdPartyApplicationUrl = jobPostingDto.ThirdPartyApplicationUrl;
                jobPosting.ThirdPartyApply = jobPostingDto.ThirdPartyApply;
                jobPosting.Country = jobPostingDto.Country;
                jobPosting.City = jobPostingDto.City;
                jobPosting.Province = jobPostingDto.Province;
                jobPosting.PostalCode = jobPostingDto.PostalCode;
                jobPosting.StreetAddress = jobPostingDto.StreetAddress;
                jobPosting.ThirdPartyIdentifier = jobPostingDto.ThirdPartyIdentifier;
                jobPosting.IsPrivate = jobPostingDto.IsPrivate;
                // Update the modify date to now
                jobPosting.ModifyDate = DateTime.UtcNow;

                // Map select items 
                if (jobPostingDto.Company == null)
                    jobPosting.CompanyId = null;
                else if (jobPostingDto.Company?.CompanyGuid != jobPosting.Company?.CompanyGuid)
                {
                    Company Company = CompanyFactory.GetCompanyByGuid(repositoryWrapper, jobPostingDto.Company.CompanyGuid).Result;
                    if (Company != null)
                        jobPosting.CompanyId = Company.CompanyId;
                    else
                        jobPosting.CompanyId = 0;
                }

                if (jobPostingDto.Industry == null)
                    jobPosting.IndustryId = null;
                else if (jobPostingDto.Industry?.IndustryGuid != jobPosting.Industry?.IndustryGuid)
                {
                    Industry industry = IndustryFactory.GetIndustryByGuid(repositoryWrapper, jobPostingDto.Industry.IndustryGuid).Result;
                    if (industry != null)
                        jobPosting.IndustryId = industry.IndustryId;
                    else
                        jobPosting.IndustryId = 0;
                }

                if (jobPostingDto.JobCategory == null)
                    jobPosting.JobCategoryId = null;
                else if (jobPostingDto.JobCategory?.JobCategoryGuid != jobPosting.JobCategory?.JobCategoryGuid)
                {
                    JobCategory JobCategory = JobCategoryFactory.GetJobCategoryByGuid(repositoryWrapper, jobPostingDto.JobCategory.JobCategoryGuid).Result;
                    if (JobCategory != null)
                        jobPosting.JobCategoryId = JobCategory.JobCategoryId;
                    else
                        jobPosting.JobCategoryId = 0;
                }

                if (jobPostingDto.SecurityClearance == null)
                    jobPosting.SecurityClearanceId = null;
                else if (jobPostingDto.SecurityClearance?.SecurityClearanceGuid != jobPosting.SecurityClearance?.SecurityClearanceGuid)
                {
                    SecurityClearance SecurityClearance = SecurityClearanceFactory.GetSecurityClearanceByGuid(repositoryWrapper, jobPostingDto.SecurityClearance.SecurityClearanceGuid).Result;
                    if (SecurityClearance != null)
                        jobPosting.SecurityClearanceId = SecurityClearance.SecurityClearanceId;
                    else
                        jobPosting.SecurityClearanceId = 0;
                }

                if (jobPostingDto.EmploymentType == null)
                    jobPosting.EmploymentTypeId = null;
                else if (jobPostingDto.EmploymentType?.EmploymentTypeGuid != jobPosting.EmploymentType?.EmploymentTypeGuid)
                {
                    EmploymentType EmploymentType = EmploymentTypeFactory.GetEmploymentTypeByGuid(repositoryWrapper, jobPostingDto.EmploymentType.EmploymentTypeGuid).Result;
                    if (EmploymentType != null)
                        jobPosting.EmploymentTypeId = EmploymentType.EmploymentTypeId;
                    else
                        jobPosting.EmploymentTypeId = 0;
                }

                if (jobPostingDto.EducationLevel == null)
                    jobPosting.EducationLevelId = null;
                else if (jobPostingDto.EducationLevel?.EducationLevelGuid != jobPosting.EducationLevel?.EducationLevelGuid)
                {
                    EducationLevel EducationLevel = EducationLevelFactory.GetEducationLevelByGuid(repositoryWrapper, jobPostingDto.EducationLevel.EducationLevelGuid).Result;
                    if (EducationLevel != null)
                        jobPosting.EducationLevelId = EducationLevel.EducationLevelId;
                    else
                        jobPosting.EducationLevelId = 0;
                }

                if (jobPostingDto.ExperienceLevel == null)
                    jobPosting.ExperienceLevelId = null;
                else if (jobPostingDto.ExperienceLevel?.ExperienceLevelGuid != jobPosting.ExperienceLevel?.ExperienceLevelGuid)
                {
                    ExperienceLevel ExperienceLevel = ExperienceLevelFactory.GetExperienceLevelByGuid(repositoryWrapper, jobPostingDto.ExperienceLevel.ExperienceLevelGuid).Result;
                    if (ExperienceLevel != null)
                        jobPosting.ExperienceLevelId = ExperienceLevel.ExperienceLevelId;
                    else
                        jobPosting.ExperienceLevelId = 0;
                }


                if (jobPostingDto.CompensationType == null)
                    jobPosting.CompensationTypeId = null;
                else if (jobPostingDto.CompensationType?.CompensationTypeGuid != jobPosting.CompensationType?.CompensationTypeGuid)
                {
                    CompensationType CompensationType = CompensationTypeFactory.GetCompensationTypeByGuid(repositoryWrapper, jobPostingDto.CompensationType.CompensationTypeGuid).Result;
                    if (CompensationType != null)
                        jobPosting.CompensationTypeId = CompensationType.CompensationTypeId;
                    else
                        jobPosting.CompensationTypeId = 0;
                }


                repositoryWrapper.SaveAsync().Wait();

                // update associated job posting skills
                JobPostingFactory.UpdateJobPostingSkills(repositoryWrapper, jobPosting.JobPostingId, jobPostingDto.JobPostingSkills);

                // index active jobs in cloud talent 
                if (jobPosting.JobStatus == (int)JobPostingStatus.Active)
                {
                    // Check to see if the job has been indexed into google 
                    if (string.IsNullOrEmpty(jobPosting.CloudTalentUri) == false)
                        _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentUpdateJob(jobPosting.JobPostingGuid));
                    else
                        _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(jobPosting.JobPostingGuid));
                }
                return true;

            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                return false;

            }
        }
    }
}
