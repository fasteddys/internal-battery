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
        private readonly CloudTalent _cloudTalent = null;
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ICompanyService _companyService;
        private readonly ISubscriberService _subscriberService;
        private readonly IMemoryCacheService _cache;
        public JobService(IServiceProvider services, IHangfireService hangfireService)
        {
            _services = services;

             _db = _services.GetService<UpDiddyDbContext>();
            _syslog = _services.GetService<ILogger<JobService>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _mapper = _services.GetService<IMapper>();
            _sysEmail = _services.GetService<ISysEmail>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _companyService=services.GetService<ICompanyService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _cache = services.GetService<IMemoryCacheService>();
            _hangfireService = hangfireService;
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper,_subscriberService);
        }


      


        public async Task<JobSearchSummaryResultDto> SummaryJobSearch(IQueryCollection query)
        {

            string cacheKey = Utils.QueryParamsToCacheKey(query);
            JobSearchSummaryResultDto rVal = (JobSearchSummaryResultDto) _cache.GetCacheValue(cacheKey);
            if ( rVal == null )
            {
                int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
                JobQueryDto jobQuery = JobQueryHelper.CreateSummaryJobQuery(PageSize, query);
                rVal = _cloudTalent.JobSummarySearch(jobQuery);
                _cache.SetCacheValue<JobSearchSummaryResultDto>(cacheKey, rVal);

            }

            return rVal; ;
        }



        public async Task ReferJobToFriend(JobReferralDto jobReferralDto)
        {
            var jobReferralGuid=await SaveJobReferral(jobReferralDto);
            await SendReferralEmail(jobReferralDto, jobReferralGuid);
        }


        public async Task UpdateJobReferral(string referrerCode, string subscriberGuid)
        {
            //get jobReferral instance to update
            var jobReferral=await _repositoryWrapper.JobReferralRepository.GetJobReferralByGuid(Guid.Parse(referrerCode));

            //get subscriber using subscriberGuid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(Guid.Parse(subscriberGuid));

            if(jobReferral!=null)
            {
                jobReferral.RefereeId = subscriber.SubscriberId;
                //update JobReferral
                await _repositoryWrapper.JobReferralRepository.UpdateJobReferral(jobReferral);
            }          
        }

        private async Task<Guid> SaveJobReferral(JobReferralDto jobReferralDto)
        {
            Guid jobReferralGuid=Guid.Empty;
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
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return jobReferralGuid;
        }

        private async Task SendReferralEmail(JobReferralDto jobReferralDto, Guid jobReferralGuid)
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

        public async Task<JobSearchResultDto> GetJobsByLocationAsync(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum,IQueryCollection query)
        {
            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobQueryDto jobQuery = JobQueryHelper.CreateJobQuery(Country, Province, City, Industry, JobCategory, Skill, PageNum, PageSize, query);
            JobSearchResultDto jobSearchResult = _cloudTalent.JobSearch(jobQuery);

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
            if(jobSearchResult.Jobs!=null && jobSearchResult.Jobs.Count>0)
            {
                // don't let this stop job search from returning
                ClientEvent ce = await _cloudTalent.CreateClientEventAsync(jobSearchResult.RequestId, ClientEventType.Impression, jobSearchResult.Jobs.Select(job => job.CloudTalentUri).ToList<string>());
                jobSearchResult.ClientEventId = ce.EventId;
            }

            return jobSearchResult;
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

        public async Task SaveJobAlert(Guid job, Guid subscriberGuid)
        {
            throw new NotImplementedException();
        }
    }
}
