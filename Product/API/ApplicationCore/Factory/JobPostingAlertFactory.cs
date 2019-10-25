
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public static class JobPostingAlertFactory
    {
        public static bool DeleteJobPostingAlert(IRepositoryWrapper repository, ILogger syslog, Guid jobPostingAlertGuid, IHangfireService _hangfireService)
        {
            bool result = true;
            try
            {
                JobPostingAlert existingJobPostingAlert = repository.JobPostingAlertRepository.GetJobPostingAlert(jobPostingAlertGuid).Result;
                existingJobPostingAlert.ModifyDate = DateTime.UtcNow;
                existingJobPostingAlert.ModifyGuid = Guid.Empty;
                existingJobPostingAlert.IsDeleted = 1;
                repository.JobPostingAlertRepository.Update(existingJobPostingAlert);
                repository.JobPostingAlertRepository.SaveAsync().Wait();
                _hangfireService.RemoveIfExists($"jobPostingAlert:{jobPostingAlertGuid}");
            }
            catch (Exception e)
            {
                result = false;
                syslog.Log(LogLevel.Information, $"**** JobPostingAlertFactory.DeleteJobPostingAlert encountered an exception; jobPostingAlertGuid: {jobPostingAlertGuid}, message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
            return result;
        }

        public static List<JobPostingAlertDto> GetJobPostingAlertsBySubscriber(IRepositoryWrapper repository, ILogger syslog, Guid subscriberGuid)
        {
            List<JobPostingAlertDto> jobPostingAlerts = null;
            try
            {

                jobPostingAlerts = repository.JobPostingAlertRepository.GetAllJobPostingAlertsBySubscriber(subscriberGuid).Result
                    .Select(jpa => new JobPostingAlertDto()
                    {
                        Description = jpa.Description,
                        ExecutionDayOfWeek = jpa.ExecutionDayOfWeek,
                        ExecutionHour = jpa.ExecutionHour,
                        ExecutionMinute = jpa.ExecutionMinute,
                        Frequency = jpa.Frequency.ToString(),
                        JobPostingAlertGuid = jpa.JobPostingAlertGuid,
                        // JobQuery = jpa.JobQueryDto,
                        Subscriber = new SubscriberDto()
                        {
                            SubscriberGuid = jpa.Subscriber.SubscriberGuid
                        }
                    }).ToList();
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Information, $"**** JobPostingAlertFactory.GetJobPostingAlertsBySubscriber encountered an exception; subscriberGuid: {subscriberGuid}, message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }

            return jobPostingAlerts;
        }

        public static bool SaveJobPostingAlert(IRepositoryWrapper repository, ILogger syslog, JobPostingAlertDto jobPostingAlertDto, IHangfireService _hangfireService)
        {
            bool result = true;
            Guid? jobPostingAlertGuid = null;
            try
            {   
                // convert localdate string to datetime
                DateTime localDate = DateTime.Parse(jobPostingAlertDto.LocalDate);
                // construct a datetime object that represents the next local execution time selected by user
                DateTime localExecutionDate = new DateTime(
                    localDate.Year, 
                    localDate.Month, 
                    jobPostingAlertDto.Frequency == "Weekly" ? localDate.Next(jobPostingAlertDto.ExecutionDayOfWeek.Value).Day : localDate.Day,
                    jobPostingAlertDto.ExecutionHour, 
                    jobPostingAlertDto.ExecutionMinute, 
                    0);                
                // adjust day, hour, and minute based on time zone offset for UTC
                var timeZoneOffset = new TimeSpan(0, jobPostingAlertDto.TimeZoneOffset, 0);                
                // calculate the utc execution date
                DateTime utcExecutionDate = localExecutionDate.Add(timeZoneOffset);

                // construct the Cron schedule for job alert
                string cronSchedule = null;

                switch (jobPostingAlertDto.Frequency)
                {
                    case "Daily":
                        cronSchedule = Cron.Daily(utcExecutionDate.Hour, utcExecutionDate.Minute);
                        break;
                    case "Weekly":
                        cronSchedule = Cron.Weekly(utcExecutionDate.DayOfWeek, utcExecutionDate.Hour, utcExecutionDate.Minute);
                        break;
                    default:
                        throw new NotSupportedException($"Unrecognized value for 'Frequency' parameter: {jobPostingAlertDto.Frequency}");
                }

                if (jobPostingAlertDto.JobPostingAlertGuid != null && jobPostingAlertDto.JobPostingAlertGuid != Guid.Empty)
                {
                    // existing alert 
                    jobPostingAlertGuid = jobPostingAlertDto.JobPostingAlertGuid;
                    var existingJobPostingAlert = repository.JobPostingAlertRepository.GetJobPostingAlert(jobPostingAlertDto.JobPostingAlertGuid.Value).Result;

                    if (existingJobPostingAlert.Subscriber.SubscriberGuid != jobPostingAlertDto.Subscriber.SubscriberGuid.Value)
                        throw new ApplicationException("An attempt was made to modify a job alert that is owned by another subscriber!");
                    existingJobPostingAlert.Description = jobPostingAlertDto.Description;
                    existingJobPostingAlert.ExecutionDayOfWeek = jobPostingAlertDto?.Frequency == "Weekly" ? (DayOfWeek?)utcExecutionDate.DayOfWeek : null;
                    existingJobPostingAlert.ExecutionHour = utcExecutionDate.Hour;
                    existingJobPostingAlert.ExecutionMinute = utcExecutionDate.Minute;
                    existingJobPostingAlert.Frequency = (Frequency)Enum.Parse(typeof(Frequency), jobPostingAlertDto.Frequency);
                    existingJobPostingAlert.ModifyDate = DateTime.UtcNow;
                    existingJobPostingAlert.ModifyGuid = Guid.Empty;
                }
                else
                {
                    // new alert
                    jobPostingAlertGuid = Guid.NewGuid();
                    int subscriberId = repository.SubscriberRepository.GetSubscriberByGuidAsync(jobPostingAlertDto.Subscriber.SubscriberGuid.Value).Result.SubscriberId;
                    repository.JobPostingAlertRepository.Create(new JobPostingAlert()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        Description = jobPostingAlertDto.Description,
                        ExecutionDayOfWeek = jobPostingAlertDto?.Frequency == "Weekly" ? (DayOfWeek?)utcExecutionDate.DayOfWeek : null,
                        ExecutionHour = utcExecutionDate.Hour,
                        ExecutionMinute = utcExecutionDate.Minute,
                        Frequency = (Frequency)Enum.Parse(typeof(Frequency), jobPostingAlertDto.Frequency),
                        IsDeleted = 0,
                        JobPostingAlertGuid = jobPostingAlertGuid.Value,
                        JobQueryDto = JObject.FromObject(jobPostingAlertDto.JobQuery),
                        SubscriberId = subscriberId
                    });
                }

                repository.JobPostingAlertRepository.SaveAsync().Wait();
                _hangfireService.AddOrUpdate<ScheduledJobs>($"jobPostingAlert:{jobPostingAlertGuid}", sj => sj.ExecuteJobPostingAlert(jobPostingAlertGuid.Value), cronSchedule);
            }
            catch (Exception e)
            {
                result = false;
                syslog.Log(LogLevel.Information, $"**** JobPostingAlertFactory.SaveJobPostingAlert encountered an exception; jobPostingAlertGuid: {jobPostingAlertGuid}, message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
            return result;
        }
    }
}
