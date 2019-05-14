using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Authorization
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private IHostingEnvironment Environment;
        public HangfireAuthorizationFilter(IHostingEnvironment _Environment)
        {
            Environment = _Environment;
        }
        public bool Authorize(DashboardContext context)
        {
            if (Environment.IsDevelopment() || Environment.IsStaging())
                return true;
            return false;
        }
    }
}
