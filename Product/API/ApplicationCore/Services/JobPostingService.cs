using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Helpers;
using UpDiddyApi.Workflow;
using UpDiddyApi.Helpers.Job;
using System.Data.SqlClient;
using System.Data;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobPostingService : IJobPostingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IHangfireService _hangfireService;
        private readonly ILogger _syslog;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public JobPostingService(IServiceProvider services, IRepositoryWrapper repositoryWrapper, IMapper mapper, IHangfireService hangfireService, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _hangfireService = hangfireService;
            _syslog = services.GetService<ILogger<JobPostingService>>();
            _configuration = configuration;

        }

        public async Task<List<RelatedJobDto>> GetJobsByCourses(List<Guid> courseGuids, int limit, int offset, Guid? subscriberGuid = null)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetJobsByCourses(courseGuids, limit, offset, subscriberGuid);
        }

        public async Task<List<RelatedJobDto>> GetJobsByCourse(Guid courseGuid, int limit, int offset, Guid? subscriberGuid = null)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetJobsByCourse(courseGuid, limit, offset, subscriberGuid);
        }

        public async Task<List<CareerPathJobDto>> GetCareerPathRecommendations(int limit, int offset, Guid subscriberGuid)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            List<CareerPathJobDto> jobDto = new List<CareerPathJobDto>();
            if (subscriber.TopicId != null)
            {
                var topic = await _repositoryWrapper.Topic.GetById(subscriber.TopicId.Value);
                var jobs = await _repositoryWrapper.StoredProcedureRepository.GetJobsByTopic(topic.TopicGuid.Value, limit, offset, subscriberGuid);
                jobDto = _mapper.Map<List<CareerPathJobDto>>(jobs);
            }
            return jobDto;
        }

        public async Task<List<RelatedJobDto>> GetJobsBySubscriber(Guid subscriberGuid, int limit, int offset)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetJobsBySubscriber(subscriberGuid, limit, offset);
        }

        /// <summary>
        /// Gets the job count based on state (province). It utilizes two types of enums (state prefix and state name)
        /// because the jobposting's province column has data that spells out state names and that uses abbreviations.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync()
        {
            var jobCount = await _repositoryWrapper.StoredProcedureRepository.GetJobCountPerProvince();
            var provinces = jobCount.Select(x => x.Province).Distinct();
            var jobCountDto = new List<JobPostingCountDto>();
            Enums.ProvinceName stateNameEnum;
            Enums.ProvincePrefix statePrefixEnum;
            foreach (var province in provinces)
            {
                var str = province.Trim().Replace(" ", "").ToUpper();
                string statePrefix = string.Empty;
                if (Enum.TryParse(str, out statePrefixEnum))
                {
                    statePrefix = str;
                }
                else if (Enum.TryParse(str, out stateNameEnum))
                {
                    Enums.ProvinceName value = (Enums.ProvinceName)(int)stateNameEnum;
                    statePrefix = value.ToString();
                }
                if (!String.IsNullOrEmpty(statePrefix))
                {
                    var distinctCompanies = jobCount
                    .Where(x => x.Province == province)
                    .Select(m => new { m.CompanyName, m.CompanyGuid, m.Count })
                    .OrderByDescending(x => x.Count);
                    List<JobPostingCompanyCountDto> companyCountdto = new List<JobPostingCompanyCountDto>();
                    foreach (var company in distinctCompanies)
                    {
                        companyCountdto.Add(new JobPostingCompanyCountDto()
                        {
                            CompanyGuid = company.CompanyGuid,
                            CompanyName = company.CompanyName,
                            JobCount = company.Count
                        });
                    }

                    if (companyCountdto.Count > 0)
                    {
                        var total = jobCount.Where(x => x.Province == province).Sum(s => s.Count);
                        jobCountDto.Add(new JobPostingCountDto(statePrefix, companyCountdto, total));
                    }
                }
            }
            return jobCountDto;
        }

        public async Task<List<JobDto>> GetSubscriberJobFavorites(int SubscriberId)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetSubscriberJobFavorites(SubscriberId);
        }

        public async Task<Guid> CreateJobPosting(JobCrudDto jobCrudDto)
        {
            if (jobCrudDto == null)
                throw new NotFoundException("JobPostingService.CreateJobPosting: No jobCrudDto was passed");

            Tuple<Guid?, string> createJobPostingResult = null;
            try
            {
                this.OverridePostingAndExpirationDates(ref jobCrudDto);
                createJobPostingResult = await _repositoryWrapper.StoredProcedureRepository.CreateJobPosting(jobCrudDto);
                if (jobCrudDto.JobStatus == (int)JobPostingStatus.Active)
                    _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(createJobPostingResult.Item1.Value));
            }
            catch (Exception e)
            {
                _syslog.LogInformation($"JobPostingService.CreateJobPosting encountered an exception: {e.Message}");
                throw new JobPostingCreation("An unexpected error occurred while attempting to create the job posting");
            }

            if (createJobPostingResult.Item1.HasValue)
                return createJobPostingResult.Item1.Value;
            else
                throw new FailedValidationException(createJobPostingResult.Item2);
        }

        public async Task<Guid> CreateJobPostingForSubscriber(Guid subscriberGuid, JobCrudDto jobCrudDto)
        {
            Recruiter recruiter = _repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Where(r => r.IsDeleted == 0 && r.RecruiterGuid == jobCrudDto.RecruiterGuid)
                .FirstOrDefault();

            // For now only allow posting to be created by their creator
            if (recruiter.Subscriber.SubscriberGuid != subscriberGuid)
                throw new JobPostingCreation("Recruiters may not post jobs for other recruiters");

            return await this.CreateJobPosting(jobCrudDto);
        }

        public async Task<bool> UpdateJobPosting(JobCrudDto jobCrudDto)
        {
            string updateJobPostingResult = null;
            try
            {
                this.OverridePostingAndExpirationDates(ref jobCrudDto);
                updateJobPostingResult = await _repositoryWrapper.StoredProcedureRepository.UpdateJobPosting(jobCrudDto);
                if (jobCrudDto.JobStatus == (int)JobPostingStatus.Active)
                    _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentUpdateJob(jobCrudDto.JobPostingGuid.Value));
            }
            catch (Exception e)
            {
                _syslog.LogInformation($"JobPostingService.UpdateJobPosting encountered an exception: {e.Message}");
                throw new JobPostingUpdate("An unexpected error occurred while attempting to update the job posting");
            }

            if (!string.IsNullOrWhiteSpace(updateJobPostingResult))
                throw new JobPostingUpdate(updateJobPostingResult);

            return true;
        }

        public async Task<bool> UpdateJobPostingForSubscriber(Guid subscriberGuid, JobCrudDto jobCrudDto)
        {
            Recruiter recruiter = _repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Where(r => r.IsDeleted == 0 && r.RecruiterGuid == jobCrudDto.RecruiterGuid)
                .FirstOrDefault();

            // For now only allow posting to be updated by their creator
            if (recruiter.Subscriber.SubscriberGuid != subscriberGuid)
                throw new JobPostingUpdate("Recruiters may not update jobs for other recruiters");

            return await this.UpdateJobPosting(jobCrudDto);
        }

        public async Task<bool> DeleteJobPosting(Guid subscriberGuid, Guid jobPostingGuid)
        {

            _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting started at: {DateTime.UtcNow.ToLongDateString()} for posting {jobPostingGuid}");

            if (jobPostingGuid == null)
                throw new NotFoundException("No job posting identifier was provided");

            string ErrorMsg = string.Empty;


            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                throw new NotFoundException($"Job posting {jobPostingGuid} does not exist");

            Recruiter recruiter = await RecruiterFactory.GetRecruiterById(_repositoryWrapper, jobPosting.RecruiterId.Value);
            if (recruiter == null)
                throw new NotFoundException($"Recruiter {jobPosting.RecruiterId.Value} rec not found");

            // check to see if the subscriber associated with the jobposting is the same as the subscriber who is requesting the delete 
            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subscriberGuid)
                throw new UnauthorizedAccessException("JobPosting owner is not specified or does not match user posting job");

            // queue a job to delete the posting from the job index and mark it as deleted in sql server
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteJob(jobPosting.JobPostingGuid));
            _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");

            return true;
        }

        public async Task<UpDiddyLib.Dto.JobPostingDto> GetJobPosting(Guid subscriberGuid, Guid jobPostingGuid)
        {

            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuid(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                throw new NotFoundException($"Job posting {jobPostingGuid} does not exist");

            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subscriberGuid)
                throw new UnauthorizedAccessException("JobPosting owner is not specified or does not match user posting job");

            return _mapper.Map<UpDiddyLib.Dto.JobPostingDto>(jobPosting);
        }

        public async Task<List<UpDiddyLib.Dto.JobPostingDto>> GetJobPostingForSubscriber(Guid subscriberGuid)
        {

            List<JobPosting> jobPostings = await JobPostingFactory.GetJobPostingsForSubscriber(_repositoryWrapper, subscriberGuid);

            return _mapper.Map<List<UpDiddyLib.Dto.JobPostingDto>>(jobPostings);
        }

        public async Task<JobCrudListDto> GetJobPostingCrudForSubscriber(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            return _mapper.Map<JobCrudListDto>(await _repositoryWrapper.StoredProcedureRepository.GetSubscriberJobPostingCruds(subscriberGuid, limit, offset, sort, order));
        }

        public async Task<JobCrudDto> GetJobPostingCrud(Guid subscriberGuid, Guid jobPostingGuid)
        {

            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuidWithRelatedObjectsAsync(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                throw new NotFoundException($"Job posting {jobPostingGuid} does not exist");

            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subscriberGuid)
                throw new UnauthorizedAccessException("JobPosting owner is not specified or does not match user posting job");

            return _mapper.Map<JobCrudDto>(jobPosting);
        }

        public async Task<JobSiteScrapeStatisticsListDto> GetJobSiteScrapeStatistics(int limit, int offset, string sort, string order)
        {
            var jobSiteScrapeStatistics = await _repositoryWrapper.StoredProcedureRepository.GetJobSiteScrapeStatistics(limit, offset, sort, order);
            return _mapper.Map<JobSiteScrapeStatisticsListDto>(jobSiteScrapeStatistics);
        }

        public async Task<bool> UpdateJobPostingSkills(Guid subscriberGuid, Guid jobPostingGuid, List<UpDiddyLib.Domain.Models.SkillDto> skills)
        {
            if (skills == null)
                throw new NotFoundException("JobPostingService.UpdateJobPostingSkills: Recruiter Guid not specified for job posting");

            Recruiter recruiter = _repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Where(r => r.IsDeleted == 0 && r.Subscriber.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();

            if (recruiter == null)
                throw new NotFoundException($"JobPostingService.UpdateJobPostingSkills: Cannot locate Recruiter {subscriberGuid}");

            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuid(_repositoryWrapper, jobPostingGuid);

            if (jobPosting == null)
                throw new NotFoundException($"Job posting {jobPostingGuid} does not exist");

            // For now only allow posting to be updatded by their creator
            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subscriberGuid)
                throw new UnauthorizedAccessException();

            await UpdateJobPostingSkills(jobPosting.JobPostingId, skills);

            return true;

        }

        public async Task<bool> UpdateJobPostingSkills(int jobPostingId, List<UpDiddyLib.Domain.Models.SkillDto> jobPostingSkills)
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

            var rowsAffected = _repositoryWrapper.JobPostingSkillRepository.ExecuteSQL(@"
                EXEC [dbo].[System_Update_JobPostingSkills] 
                    @JobPostingId,
	                @SkillGuids", spParams);

            return true;
        }

        public async Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetJobPostingSkills(Guid subscriberGuid, Guid jobPostingGuid)
        {

            Recruiter recruiter = _repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Where(r => r.IsDeleted == 0 && r.Subscriber.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();

            if (recruiter == null)
                throw new NotFoundException($"JobPostingService.UpdateJobPostingSkills: Cannot locate Recruiter {subscriberGuid}");

            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuid(_repositoryWrapper, jobPostingGuid);

            if (jobPosting == null)
                throw new NotFoundException($"Job posting {jobPostingGuid} does not exist");

            // For now only allow posting to be updatded by their creator
            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subscriberGuid)
                throw new InvalidOperationException("Requestor is not owner of posting");

            List<UpDiddyLib.Domain.Models.SkillDto> rVal = null;

            List<JobPostingSkill> jobSkills = _repositoryWrapper.JobPostingSkillRepository.GetByJobPostingId(jobPosting.JobPostingId);

            rVal = _mapper.Map<List<UpDiddyLib.Domain.Models.SkillDto>>(jobSkills);

            return rVal;

        }

        #region Private methods

        private void OverridePostingAndExpirationDates(ref JobCrudDto jobCrudDto)
        {
            int postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);

            if (jobCrudDto.PostingDateUTC < DateTime.UtcNow)
                jobCrudDto.PostingDateUTC = DateTime.UtcNow;
            if (jobCrudDto.PostingExpirationDateUTC < DateTime.UtcNow)
            {
                jobCrudDto.PostingExpirationDateUTC = DateTime.UtcNow.AddDays(postingTTL);
            }
            if (jobCrudDto.ApplicationDeadlineUTC < DateTime.UtcNow)
            {
                jobCrudDto.ApplicationDeadlineUTC = DateTime.UtcNow.AddDays(postingTTL);
            }
        }

        [Obsolete("This method should not be used because it does not properly validate related entities for jobs", true)]
        private Guid PostJob(IRepositoryWrapper repositoryWrapper, int recruiterId, UpDiddyLib.Dto.JobPostingDto jobPostingDto, ref Guid newPostingGuid, ref string ErrorMsg, ILogger syslog, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, IHangfireService _hangfireService)
        {
            Guid rVal = Guid.Empty;
            int postingTTL = int.Parse(configuration["JobPosting:PostingTTLInDays"]);

            if (jobPostingDto == null)
            {
                ErrorMsg = "JobPosting is required";
                return rVal;
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
                return rVal;
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

            return jobPosting.JobPostingGuid;
        }

        private async Task<UpDiddyLib.Dto.JobPostingDto> MapPostingCrudToJobPosting(Guid subscriberGuid, JobCrudDto jobCrudDto, Recruiter recruiter)
        {
            // map base properties
            UpDiddyLib.Dto.JobPostingDto jobPostingDto = _mapper.Map<UpDiddyLib.Dto.JobPostingDto>(jobCrudDto);
            // map related entities 
            jobPostingDto.Recruiter = new RecruiterDto()
            {
                RecruiterGuid = jobCrudDto.RecruiterGuid,
                FirstName = recruiter.FirstName,
                LastName = recruiter.LastName,
                PhoneNumber = recruiter.PhoneNumber,
                Email = recruiter.Email,

                Subscriber = new UpDiddyLib.Dto.SubscriberDto()
                {
                    SubscriberGuid = subscriberGuid
                }
            };

            // map company no need to check if guid exists since that been validated above
            jobPostingDto.Company = new CompanyDto()
            {
                CompanyGuid = jobCrudDto.CompanyGuid
            };

            if (jobCrudDto.IndustryGuid != null && jobCrudDto.IndustryGuid != Guid.Empty)
                jobPostingDto.Industry = new UpDiddyLib.Dto.IndustryDto()
                {
                    IndustryGuid = jobCrudDto.IndustryGuid
                };


            if (jobCrudDto.JobCategoryGuid != null && jobCrudDto.JobCategoryGuid != Guid.Empty)
                jobPostingDto.JobCategory = new JobCategoryDto()
                {
                    JobCategoryGuid = jobCrudDto.JobCategoryGuid
                };

            if (jobCrudDto.ExperienceLevelGuid != null && jobCrudDto.ExperienceLevelGuid != Guid.Empty)
                jobPostingDto.ExperienceLevel = new UpDiddyLib.Dto.ExperienceLevelDto()
                {
                    ExperienceLevelGuid = jobCrudDto.ExperienceLevelGuid
                };


            if (jobCrudDto.EducationLevelGuid != null && jobCrudDto.EducationLevelGuid != Guid.Empty)
                jobPostingDto.EducationLevel = new UpDiddyLib.Dto.EducationLevelDto()
                {
                    EducationLevelGuid = jobCrudDto.EducationLevelGuid
                };


            if (jobCrudDto.CompensationTypeGuid != null && jobCrudDto.CompensationTypeGuid != Guid.Empty)
                jobPostingDto.CompensationType = new UpDiddyLib.Dto.CompensationTypeDto()
                {
                    CompensationTypeGuid = jobCrudDto.CompensationTypeGuid
                };


            if (jobCrudDto.SecurityClearanceGuid != null && jobCrudDto.SecurityClearanceGuid != Guid.Empty)
                jobPostingDto.SecurityClearance = new UpDiddyLib.Dto.SecurityClearanceDto()
                {
                    SecurityClearanceGuid = jobCrudDto.SecurityClearanceGuid
                };


            if (jobCrudDto.EmploymentTypeGuid != null && jobCrudDto.EmploymentTypeGuid != Guid.Empty)
                jobPostingDto.EmploymentType = new UpDiddyLib.Dto.EmploymentTypeDto()
                {
                    EmploymentTypeGuid = jobCrudDto.EmploymentTypeGuid
                };


            return jobPostingDto;


        }

        #endregion
    }
}