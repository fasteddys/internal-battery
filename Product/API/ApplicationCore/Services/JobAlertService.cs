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

        private readonly IHangfireService _hangfireService;

        public JobAlertService(IHangfireService hangfireService, IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _hangfireService = hangfireService;
        }

        public async Task CreateJobAlert(Guid subscriberGuid, JobAlertDto jobAlertDto)
        {
            try
            {
                var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);

                if (subscriber == null)
                    throw new NotFoundException("Subscriber not found");

                JobQueryDto jobQueryDto = CreateJobQueryDto(jobAlertDto);
                var alerts = await _repositoryWrapper.JobPostingAlertRepository.GetAllJobPostingAlertsBySubscriber(subscriberGuid);

                if (alerts.ToList().Count >= 4)
                    throw new MaximumReachedException("Subscriber has exceeded the maximum number of job exception");

                DateTime localExecutionDate = CreateLocalExecutionDate(jobAlertDto.Frequency, jobAlertDto.ExecutionDayOfWeek);
                string cronSchedule = CreateCronSchedule(jobAlertDto.Frequency, localExecutionDate);
                int subscriberId = subscriber.SubscriberId;
                Guid jobPostingAlertGuid = Guid.NewGuid();
                JobPostingAlert jobPostingAlert = new JobPostingAlert()
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
                };

                await _repositoryWrapper.JobPostingAlertRepository.Create(jobPostingAlert);
                await _repositoryWrapper.SaveAsync();
                _hangfireService.AddOrUpdate<ScheduledJobs>($"jobPostingAlert:{jobPostingAlertGuid}", sj => sj.ExecuteJobPostingAlert(jobPostingAlertGuid), cronSchedule);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task UpdateJobAlert(Guid subscriberGuid, Guid jobAlertGuid, JobAlertDto jobAlertDto)
        {
            JobPostingAlert jobPostingAlert = await _repositoryWrapper.JobPostingAlertRepository.GetJobPostingAlertBySubscriberGuidAndJobPostingAlertGuid(subscriberGuid, jobAlertGuid);
            if (jobPostingAlert == null)
            {
                throw new NotFoundException("Job Posting Alert not found");
            }
            JobQueryDto jobQueryDto = CreateJobQueryDto(jobAlertDto);
            DateTime localExecutionDate = CreateLocalExecutionDate(jobAlertDto.Frequency, jobAlertDto.ExecutionDayOfWeek);
            string cronSchedule = CreateCronSchedule(jobAlertDto.Frequency, localExecutionDate);
            jobPostingAlert.Description = jobAlertDto.Description;
            jobPostingAlert.JobQueryDto = JObject.FromObject(jobQueryDto);
            jobPostingAlert.ExecutionDayOfWeek = jobAlertDto?.Frequency == "Weekly" ? (DayOfWeek?)localExecutionDate.DayOfWeek : null;
            jobPostingAlert.ExecutionHour = localExecutionDate.Hour;
            jobPostingAlert.ExecutionMinute = localExecutionDate.Minute;
            jobPostingAlert.Frequency = (Frequency)Enum.Parse(typeof(Frequency), jobAlertDto.Frequency);
            jobPostingAlert.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.JobPostingAlertRepository.Update(jobPostingAlert);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task<List<JobAlertDto>> GetJobAlert(Guid subscriberGuid)
        {
            var result = await _repositoryWrapper.JobPostingAlertRepository.GetAllJobPostingAlertsBySubscriber(subscriberGuid);
            var jobPostingAlerts = result
                   .Select(jpa => new JobAlertDto()
                   {
                       Description = jpa.Description,
                       ExecutionDayOfWeek = jpa.ExecutionDayOfWeek.HasValue ? jpa.ExecutionDayOfWeek.Value.ToString() : null,
                       Frequency = jpa.Frequency.ToString(),
                       JobAlertGuid = jpa.JobPostingAlertGuid,
                       Keywords = JsonConvert.DeserializeObject<JobQueryDto>(jpa.JobQueryDto.ToString()).Keywords,
                       Location = JsonConvert.DeserializeObject<JobQueryDto>(jpa.JobQueryDto.ToString()).Location,
                   }).ToList();

            return jobPostingAlerts;
        }

        public async Task DeleteJobAlert(Guid subscriberGuid, Guid jobAlertGuid)
        {
            var alert = await _repositoryWrapper.JobPostingAlertRepository.GetJobPostingAlert(jobAlertGuid);
            if (alert == null)
            {
                throw new NotFoundException("Job Alert could not be found");
            }

            if (alert.Subscriber.SubscriberGuid != subscriberGuid)
            {
                throw new UnauthorizedAccessException();
            }

            alert.IsDeleted = 1;
            _repositoryWrapper.JobPostingAlertRepository.Update(alert);
            await _repositoryWrapper.JobPostingAlertRepository.SaveAsync();
            _hangfireService.RemoveIfExists($"jobPostingAlert:{jobAlertGuid}");
        }

        #region Private Helper Functions

        private JobQueryDto CreateJobQueryDto(JobAlertDto alertDto)
        {
            JobQueryDto jobQueryDto = new JobQueryDto();
            jobQueryDto.Keywords = alertDto.Keywords;
            jobQueryDto.Location = string.IsNullOrEmpty(alertDto.Location) ? string.Empty : alertDto.Location;
            jobQueryDto.ExcludeCustomProperties = 1;
            jobQueryDto.ExcludeFacets = 1;
            jobQueryDto.PageSize = 20;
            jobQueryDto.NumPages = 1;
            return jobQueryDto;
        }

        private DateTime CreateLocalExecutionDate(string frequency, string executionDayOfWeek)
        {
            DateTime utcDate = DateTime.UtcNow;
            DayOfWeek dayOfWeek;
            bool isValid = DayOfWeek.TryParse(executionDayOfWeek, out dayOfWeek);
            if (!isValid)
            {
                throw new NotSupportedException($"Unrecognized value for 'ExecutionDayOfWeek' parameter: {executionDayOfWeek}");
            }
            DateTime localExecutionDate = new DateTime(
                   utcDate.Year,
                   utcDate.Month,
                   frequency == "Weekly" ? utcDate.Next(dayOfWeek).Day : utcDate.Day,
                   utcDate.Hour,
                   utcDate.Minute,
                   0);
            return localExecutionDate;
        }

        private string CreateCronSchedule(string frequency, DateTime localExecutionDate)
        {
            string cronSchedule = null;

            switch (frequency)
            {
                case "Daily":
                    cronSchedule = Cron.Daily(localExecutionDate.Hour, localExecutionDate.Minute);
                    break;
                case "Weekly":
                    cronSchedule = Cron.Weekly(localExecutionDate.DayOfWeek, localExecutionDate.Hour, localExecutionDate.Minute);
                    break;
                default:
                    throw new NotSupportedException($"Unrecognized value for 'Frequency' parameter: {frequency}");
            }

            return cronSchedule;
        }
        #endregion
    }
}
