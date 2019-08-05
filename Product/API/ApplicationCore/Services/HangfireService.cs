using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class HangfireService : IHangfireService
    {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private bool IsPreliminaryEnvironment;

        public HangfireService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _configuration = configuration;
            IsPreliminaryEnvironment = Boolean.Parse(_configuration["Environment:IsPreliminary"]);
        }

        public void AddOrUpdate<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            if (IsPreliminaryEnvironment)
                return;

            RecurringJob.AddOrUpdate<T>(recurringJobId, methodCall, cronExpression);
        }

        public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            if (IsPreliminaryEnvironment)
                return string.Empty;

            return BackgroundJob.Enqueue<T>(methodCall);
        }

        public string Enqueue(Expression<Func<Task>> methodCall)
        {
            if (IsPreliminaryEnvironment)
                return string.Empty;

            return BackgroundJob.Enqueue(methodCall);
        }

        public string Enqueue<T>(Expression<Action<T>> methodCall)
        {
            if (IsPreliminaryEnvironment)
                return string.Empty;

            return BackgroundJob.Enqueue<T>(methodCall);
        }

        public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            if (IsPreliminaryEnvironment)
                return string.Empty;

            return BackgroundJob.Schedule<T>(methodCall, delay);
        }

        public void RemoveIfExists(string recurringJobId)
        {
            if (IsPreliminaryEnvironment)
                return;

            RecurringJob.RemoveIfExists(recurringJobId);
        }


    }
}
