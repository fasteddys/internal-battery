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
using Newtonsoft.Json;
using UpDiddyApi.Workflow;
using Newtonsoft.Json.Linq;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobAlertService : IJobAlertService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        private IHangfireService _hangfireService;

        public JobAlertService(HangfireService hangfireService, IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _hangfireService = hangfireService;
        }

        public async Task CreateJobAlert(Guid subscriberGuid, JobAlertDto jobAlertDto)
        {

            try
            {
                JobQueryDto jobQueryDto = new JobQueryDto();
                jobQueryDto.Keywords = jobAlertDto.Keywords;
                jobQueryDto.Location = jobQueryDto.Location;
                var alerts = await _repositoryWrapper.JobPostingAlertRepository.GetAllJobPostingAlertsBySubscriber(subscriberGuid);
                if (alerts.ToList().Count >= 4)
                {
                    throw new MaximumReachedException("Subscriber has exceeded the maximum number of job exception");
                }

                DateTime utcDate = DateTime.UtcNow;
                DateTime localExecutionDate = new DateTime(
                    utcDate.Year,
                    utcDate.Month,
                    jobAlertDto.Frequency == "Weekly" ? utcDate.Next(jobAlertDto.ExecutionDayOfWeek.Value).Day : utcDate.Day,
                    utcDate.Hour,
                    utcDate.Minute,
                    0);


                string cronSchedule = null;

                switch (jobAlertDto.Frequency)
                {
                    case "Daily":
                        cronSchedule = Cron.Daily(localExecutionDate.Hour, localExecutionDate.Minute);
                        break;
                    case "Weekly":
                        cronSchedule = Cron.Weekly(localExecutionDate.DayOfWeek, localExecutionDate.Hour, localExecutionDate.Minute);
                        break;
                    default:
                        throw new NotSupportedException($"Unrecognized value for 'Frequency' parameter: {jobAlertDto.Frequency}");
                }
                var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
                int subscriberId = subscriber.SubscriberId;
                Guid jobPostingAlertGuid = Guid.NewGuid();
                await _repositoryWrapper.JobPostingAlertRepository.Create(new JobPostingAlert()
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    Description = jobAlertDto.Description,
                    ExecutionDayOfWeek = jobAlertDto?.Frequency == "Weekly" ? (DayOfWeek?)localExecutionDate.DayOfWeek : null,
                    ExecutionHour = localExecutionDate.Hour,
                    ExecutionMinute = localExecutionDate.Minute,
                    Frequency = (Frequency)Enum.Parse(typeof(Frequency), jobAlertDto.Frequency),
                    IsDeleted = 0,
                    JobPostingAlertGuid = jobPostingAlertGuid,
                    JobQueryDto = JObject.FromObject(jobQueryDto),
                    SubscriberId = subscriberId
                });
                await _repositoryWrapper.JobPostingAlertRepository.SaveAsync();
                _hangfireService.AddOrUpdate<ScheduledJobs>($"jobPostingAlert:{jobPostingAlertGuid}", sj => sj.ExecuteJobPostingAlert(jobPostingAlertGuid), cronSchedule);

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<List<JobAlertDto>> GetJobAlert(Guid subscriberGuid)
        {
            var result = await _repositoryWrapper.JobPostingAlertRepository.GetAllJobPostingAlertsBySubscriber(subscriberGuid);
            var jobPostingAlerts = result
                   .Select(jpa => new JobAlertDto()
                   {
                       Description = jpa.Description,
                       ExecutionDayOfWeek = jpa.ExecutionDayOfWeek,
                       Frequency = jpa.Frequency.ToString(),
                       JobAlertGuid = jpa.JobPostingAlertGuid,
                       Keywords = JsonConvert.DeserializeObject<JobQueryDto>(jpa.JobQueryDto.ToString()).Keywords,
                       Location = JsonConvert.DeserializeObject<JobQueryDto>(jpa.JobQueryDto.ToString()).Location,
                   }).ToList();
            if (jobPostingAlerts.Count == 0)
            {
                throw new NotFoundException("Job alerts could not be found");
            }
            return jobPostingAlerts;
        }

        public async Task DeleteJobAlert(Guid jobAlertGuid)
        {
            var alert = await _repositoryWrapper.JobPostingAlertRepository.GetJobPostingAlert(jobAlertGuid);
            if (alert == null)
            {
                throw new NotFoundException("Job Alert could not be found");
            }
            alert.IsDeleted = 1;
            await _repositoryWrapper.JobPostingAlertRepository.SaveAsync();
        }
    }
}
