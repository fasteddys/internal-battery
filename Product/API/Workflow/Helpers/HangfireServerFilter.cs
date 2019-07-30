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

namespace UpDiddyApi.Workflow.Helpers
{
    public class HangfireServerFilter : JobFilterAttribute, IServerFilter
    {
        public bool IsPreliminaryEnvironment;

        public HangfireServerFilter(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            IsPreliminaryEnvironment = Boolean.Parse(configuration["Environment:IsPreliminary"]);
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            // Do nothing.
        }
        public void OnPerforming(PerformingContext filterContext)
        {
            if(!IsPreliminaryEnvironment)
                filterContext.Canceled = true;
        }
    }
}
