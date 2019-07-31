using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Hangfire.Server;
using Hangfire.Common;
using Serilog;

namespace UpDiddyApi.Workflow.Helpers
{
    public class HangfireServerFilter : JobFilterAttribute, IServerFilter
    {
        public bool IsPreliminaryEnvironment;
        private ILogger _logger;

        public HangfireServerFilter(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger Logger)
        {
            IsPreliminaryEnvironment = Boolean.Parse(configuration["Environment:IsPreliminary"]);
            _logger = Logger;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            // Do nothing.
        }
        public void OnPerforming(PerformingContext filterContext)
        {
            if (IsPreliminaryEnvironment)
            {
                _logger.Information($"HangfireServerFilter: Hangfire job {filterContext.BackgroundJob.Job.ToString()} (ID: {filterContext.BackgroundJob.Id}) being cancelled due to it being run in a preliminary environment.");
                filterContext.Canceled = true;
            }
        }
    }
}
