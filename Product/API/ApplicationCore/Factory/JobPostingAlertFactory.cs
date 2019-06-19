
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public static class JobPostingAlertFactory
    {
        public static bool DeleteJobPostingAlert(IRepositoryWrapper repository, ILogger syslog, Guid jobPostingAlertGuid)
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
                RecurringJob.RemoveIfExists($"jobPostingAlert:{jobPostingAlertGuid}");
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

        public static bool SaveJobPostingAlert(IRepositoryWrapper repository, ILogger syslog, JobPostingAlertDto jobPostingAlertDto)
        {
            bool result = true;
            Guid? jobPostingAlertGuid = null;
            try
            {
                // construct the Cron schedule for job alert
                string cronSchedule = null;

                switch (jobPostingAlertDto.Frequency)
                {
                    case "Daily":
                        cronSchedule = Cron.Daily(jobPostingAlertDto.ExecutionHour, jobPostingAlertDto.ExecutionMinute);
                        break;
                    case "Weekly":
                        cronSchedule = Cron.Weekly(jobPostingAlertDto.ExecutionDayOfWeek.Value, jobPostingAlertDto.ExecutionHour, jobPostingAlertDto.ExecutionMinute);
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
                    existingJobPostingAlert.ExecutionDayOfWeek = jobPostingAlertDto.ExecutionDayOfWeek;
                    existingJobPostingAlert.ExecutionHour = jobPostingAlertDto.ExecutionHour;
                    existingJobPostingAlert.ExecutionMinute = jobPostingAlertDto.ExecutionMinute;
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
                        ExecutionDayOfWeek = jobPostingAlertDto?.Frequency == "Weekly" ? jobPostingAlertDto?.ExecutionDayOfWeek : null,
                        ExecutionHour = jobPostingAlertDto.ExecutionHour,
                        ExecutionMinute = jobPostingAlertDto.ExecutionMinute,
                        Frequency = (Frequency)Enum.Parse(typeof(Frequency), jobPostingAlertDto.Frequency),
                        IsDeleted = 0,
                        JobPostingAlertGuid = jobPostingAlertGuid.Value,
                        JobQueryDto = JObject.FromObject(jobPostingAlertDto.JobQuery),
                        SubscriberId = subscriberId
                    });
                }

                repository.JobPostingAlertRepository.SaveAsync().Wait();
                RecurringJob.AddOrUpdate<ScheduledJobs>($"jobPostingAlert:{jobPostingAlertGuid}", sj => sj.ExecuteJobPostingAlert(jobPostingAlertGuid.Value), cronSchedule);
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
