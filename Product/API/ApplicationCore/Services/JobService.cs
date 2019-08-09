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
using Hangfire;
using UpDiddyApi.ApplicationCore.Interfaces;

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
        public JobService(IServiceProvider services, IHangfireService hangfireService)
        {
            _services = services;
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _mapper = _services.GetService<IMapper>();
            _sysEmail = _services.GetService<ISysEmail>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _hangfireService = hangfireService;
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
    }
}
