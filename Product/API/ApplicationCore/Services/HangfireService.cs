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
    }
}
