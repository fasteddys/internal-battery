using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyLib.Shared;

namespace UpDiddyApi.Authorization
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private IHostingEnvironment _environment;
        private IConfiguration _configuration;
        public HangfireAuthorizationFilter(IHostingEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public bool Authorize(DashboardContext context)
        {
            if (_environment.IsDevelopment())
                return true;

            string HangfireUnlockedCookie = string.Empty;
            try
            {
                HangfireUnlockedCookie = Crypto.Decrypt(_configuration["Crypto:Key"], context.GetHttpContext().Request.Cookies["HangfireUnlocked"]);
            }
            catch(Exception)
            {
                return false;
            }
            if (HangfireUnlockedCookie == null || HangfireUnlockedCookie.Equals(string.Empty))
                return false;

            bool IsUnlocked = HangfireUnlockedCookie.Equals(_configuration["Hangfire:UnlockToken"]);
            

            if ((_environment.IsProduction() || _environment.IsStaging()) && IsUnlocked)
                return true;
            return false;
        }
    }
}
