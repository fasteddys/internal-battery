using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Helpers.Job;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Shared.GoogleJobs;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Domain.Models;
using Hangfire;
using Newtonsoft.Json.Linq;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Factory;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobService : IJobService
    {
        private readonly IServiceProvider _services;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private ISysEmail _sysEmail;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private IHangfireService _hangfireService;
        private readonly ICloudTalentService _cloudTalentService;
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ICompanyService _companyService;
        private readonly ISubscriberService _subscriberService;
        private readonly IMemoryCacheService _cache;
        public JobService(IServiceProvider services, IHangfireService hangfireService, ICloudTalentService cloudTalentService)
        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _syslog = _services.GetService<ILogger<JobService>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _mapper = _services.GetService<IMapper>();
            _sysEmail = _services.GetService<ISysEmail>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _companyService = services.GetService<ICompanyService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _cache = services.GetService<IMemoryCacheService>();
            _hangfireService = hangfireService;
            _cloudTalentService = cloudTalentService;
        }


        public async Task<JobDetailDto> GetJobDetail(Guid jobPostingGuid)
        {
            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuidWithRelatedObjectsAsync(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                throw new NotFoundException();
            if (jobPosting.IsDeleted == 1)
                throw new ExpiredJobException();

            JobDetailDto rVal = _mapper.Map<JobDetailDto>(jobPosting);
            return rVal;
        }




        public async Task<UpDiddyLib.Dto.JobPostingDto> GetJob(Guid jobPostingGuid)
        {
            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuidWithRelatedObjectsAsync(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                throw new NotFoundException();
            if (jobPosting.IsDeleted == 1)
                throw new ExpiredJobException();
            UpDiddyLib.Dto.JobPostingDto rVal = _mapper.Map<UpDiddyLib.Dto.JobPostingDto>(jobPosting);

            // set meta data for seo
            JobPostingFactory.SetMetaData(jobPosting, rVal);

            /* i would prefer to get the semantic url in automapper, but i ran into a blocker while trying to call the static util method
             * in "MapFrom" while guarding against null refs: an expression tree lambda may not contain a null propagating operator
             * .ForMember(jp => jp.SemanticUrl, opt => opt.MapFrom(src => Utils.GetSemanticJobUrlPath(src.Industry?.Name,"","","","","")))
             */

            rVal.SemanticJobPath = Utils.CreateSemanticJobPath(
                jobPosting.Industry?.Name,
                jobPosting.JobCategory?.Name,
                jobPosting.Country,
                jobPosting.Province,
                jobPosting.City,
                jobPostingGuid.ToString());

            JobQueryDto jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(jobPosting.Province, jobPosting.City, jobPosting.Title, Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]));
            JobSearchResultDto jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);

            // If jobs in same city come back less than 6, broaden search to state.
            if (jobSearchForSingleJob.JobCount < Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]))
            {
                jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(jobPosting.Province, string.Empty, jobPosting.Title, Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]));
                jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);
            }



            rVal.SimilarJobs = jobSearchForSingleJob;

            return rVal;
        }






        public async Task<JobSearchSummaryResultDto> SummaryJobSearch(IQueryCollection query)
        {

            string cacheKey = Utils.QueryParamsToCacheKey(query);
            JobSearchSummaryResultDto rVal = (JobSearchSummaryResultDto)_cache.GetCacheValue(cacheKey);
            if (rVal == null)
            {
                int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
                JobQueryDto jobQuery = JobQueryHelper.CreateSummaryJobQuery(PageSize, query);
                rVal = _cloudTalentService.JobSummarySearch(jobQuery);
                  await AssignCompanyLogoUrlToJobs(rVal.Jobs);
                _cache.SetCacheValue<JobSearchSummaryResultDto>(cacheKey, rVal);

            }

            return rVal; ;
        }

        public async Task<List<SearchTermDto>> GetKeywordSearchTermsAsync()
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetKeywordSearchTermsAsync();
        }

        public async Task<List<SearchTermDto>> GetLocationSearchTermsAsync()
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetLocationSearchTermsAsync();
        }

        public async Task ReferJobToFriend(JobReferralDto jobReferralDto)
        {
            var jobReferralGuid = await SaveJobReferral(jobReferralDto);
            SendReferralEmail(jobReferralDto, jobReferralGuid);
        }

        public async Task UpdateJobReferral(string referrerCode, string subscriberGuid)
        {
            //get jobReferral instance to update
            var jobReferral = await _repositoryWrapper.JobReferralRepository.GetJobReferralByGuid(Guid.Parse(referrerCode));

            //get subscriber using subscriberGuid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(Guid.Parse(subscriberGuid));

            if (jobReferral != null)
            {
                jobReferral.RefereeId = subscriber.SubscriberId;
                //update JobReferralreferrerCode
                await _repositoryWrapper.JobReferralRepository.UpdateJobReferral(jobReferral);
            }
        }

        private async Task<Guid> SaveJobReferral(JobReferralDto jobReferralDto)
        {
            Guid jobReferralGuid = Guid.Empty;
            try
            {
                //get JobPostingId from JobPositngGuid
                var jobPosting = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(Guid.Parse(jobReferralDto.JobPostingId));

                //get ReferrerId from ReferrerGuid
                var referrer = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(Guid.Parse(jobReferralDto.ReferrerGuid));

                //get ReferrerId from ReferrerGuid
                var referee = await _repositoryWrapper.SubscriberRepository.GetSubscriberByEmailAsync(jobReferralDto.RefereeEmailId);

                //create JobReferral
                JobReferral jobReferral = new JobReferral()
                {
                    JobReferralGuid = Guid.NewGuid(),
                    JobPostingId = jobPosting.JobPostingId,
                    ReferralId = referrer.SubscriberId,
                    RefereeId = referee?.SubscriberId,
                    RefereeEmailId = jobReferralDto.RefereeEmailId,
                    IsJobViewed = false
                };

                //set defaults
                BaseModelFactory.SetDefaultsForAddNew(jobReferral);

                //update jobReferralGuid only if Referee is new subscriber, for old subscriber we do not jobReferralCode
                jobReferralGuid = await _repositoryWrapper.JobReferralRepository.AddJobReferralAsync(jobReferral);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return jobReferralGuid;
        }

        private void SendReferralEmail(JobReferralDto jobReferralDto, Guid jobReferralGuid)
        {
            //generate jobUrl
            var referralUrl = jobReferralGuid == Guid.Empty ? jobReferralDto.ReferUrl : $"{jobReferralDto.ReferUrl}?referrerCode={jobReferralGuid}";

            _hangfireService.Enqueue(() => _sysEmail.SendTemplatedEmailAsync(
                jobReferralDto.RefereeEmailId,
                _configuration["SysEmail:Transactional:TemplateIds:JobReferral-ReferAFriend"],
                new
                {
                    firstName = jobReferralDto.RefereeName,
                    description = jobReferralDto.DescriptionEmailBody,
                    jobUrl = referralUrl
                },
               Constants.SendGridAccount.Transactional,
               null,
               null,
               null,
               null
                ));
        }

        public async Task UpdateJobViewed(string referrerCode)
        {
            //get jobReferral instance to update
            var jobReferral = await _repositoryWrapper.JobReferralRepository.GetJobReferralByGuid(Guid.Parse(referrerCode));

            if (jobReferral != null)
            {
                jobReferral.IsJobViewed = true;
                //update JobReferral
                await _repositoryWrapper.JobReferralRepository.UpdateJobReferral(jobReferral);
            }
        }

        public async Task<JobSearchResultDto> GetJobsByLocationAsync(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum, IQueryCollection query)
        {
            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobQueryDto jobQuery = JobQueryHelper.CreateJobQuery(Country, Province, City, Industry, JobCategory, Skill, PageNum, PageSize, query);
            JobSearchResultDto jobSearchResult = _cloudTalentService.JobSearch(jobQuery);

            //assign company logo urls
            await AssignCompanyLogoUrlToJobs(jobSearchResult.Jobs);

            // set common properties for an alert jobQuery and include this in the response
            jobQuery.DatePublished = null;
            jobQuery.ExcludeCustomProperties = 1;
            jobQuery.ExcludeFacets = 1;
            jobQuery.PageSize = 20;
            jobQuery.NumPages = 1;
            jobSearchResult.JobQueryForAlert = jobQuery;

            //ClientEvents are triggered only when there are jobs
            if (jobSearchResult.Jobs != null && jobSearchResult.Jobs.Count > 0)
            {
                // don't let this stop job search from returning
                ClientEvent ce = await _cloudTalentService.CreateClientEventAsync(jobSearchResult.RequestId, ClientEventType.Impression, jobSearchResult.Jobs.Select(job => job.CloudTalentUri).ToList<string>());
                jobSearchResult.ClientEventId = ce.EventId;
            }

            return jobSearchResult;
        }

        private async Task AssignCompanyLogoUrlToJobs(List<JobSummaryViewDto> jobs)
        {
            var companies = await _companyService.GetCompaniesAsync();
            foreach (var job in jobs)
            {
                var company = companies.Where(x => x.CompanyName == job.CompanyName).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(company?.LogoUrl))
                    job.CompanyLogoUrl = _configuration["StorageAccount:AssetBaseUrl"] + "Company/" + company.LogoUrl;
            }
        }

        private async Task AssignCompanyLogoUrlToJobs(List<JobViewDto> jobs)
        {
            var companies = await _companyService.GetCompaniesAsync();
            foreach (var job in jobs)
            {
                var company = companies.Where(x => x.CompanyName == job.CompanyName).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(company?.LogoUrl))
                    job.CompanyLogoUrl = _configuration["StorageAccount:AssetBaseUrl"] + "Company/" + company.LogoUrl;
            }
        }

        public async Task ShareJob(Guid job, Guid subscriber, ShareJobDto shareJobDto)
        {
            if (string.IsNullOrEmpty(shareJobDto.Email))
            {
                throw new NullReferenceException("Email cannot be empty");
            }

            Guid jobReferralGuid = Guid.Empty;

            //get JobPostingId from JobPositngGuid
            var jobPosting = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(job);
            if (jobPosting == null)
                throw new NotFoundException("job posting not found");

            //get ReferrerId from ReferrerGuid
            var referrer = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriber);

            //get ReferrerId from ReferrerGuid
            var referee = await _repositoryWrapper.SubscriberRepository.GetSubscriberByEmailAsync(shareJobDto.Email);

            //create JobReferral
            JobReferral jobReferral = new JobReferral()
            {
                JobReferralGuid = Guid.NewGuid(),
                JobPostingId = jobPosting.JobPostingId,
                ReferralId = referrer.SubscriberId,
                RefereeId = referee?.SubscriberId,
                RefereeEmailId = shareJobDto.Email,
                IsJobViewed = false
            };

            //set defaults
            BaseModelFactory.SetDefaultsForAddNew(jobReferral);

            //update jobReferralGuid only if Referee is new subscriber, for old subscriber we do not jobReferralCode
            await _repositoryWrapper.JobReferralRepository.AddJobReferralAsync(jobReferral);
        }


    }
}
