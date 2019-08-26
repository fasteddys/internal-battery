using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UpDiddy.Api;
using UpDiddyLib.Dto;

namespace UpDiddy.Authorization
{
    public class ApiGroupAuthorizationHandler : AuthorizationHandler<GroupRequirement>
    {
        IConfiguration _configuration;
        IApi _api;
        IHttpContextAccessor _accessor;
        ILogger _syslog = null; 
        IMemoryCache _memoryCache = null;

        public const string CACHE_KEY = "SubscriberGroups";

        public ApiGroupAuthorizationHandler(IApi api, IConfiguration configuration, IHttpContextAccessor contextAccessor, ILogger<ApiGroupAuthorizationHandler> sysLog, IMemoryCache memoryCache )
        {
            _api = api;
            _configuration = configuration;
            _accessor = contextAccessor;
            _syslog = sysLog; 
            _memoryCache = memoryCache;
      
        }

        private async Task<bool> CheckAuthAsync(string userId, GroupRequirement requirement)
        {

            IList<string> groups = new List<string>();
            string cachedGroups = _memoryCache.Get<string>(CACHE_KEY + userId);        
            if (cachedGroups != null)
            {
                groups = JsonConvert.DeserializeObject<List<string>>(cachedGroups);
            } else
            {
                try
                {  
                    SubscriberADGroupsDto dto = await _api.MyGroupsAsync();
                    groups = dto.groups;
                    int SubscriberGroupsCacheTimeInMinutes = int.Parse(_configuration["CareerCircle:SubscriberGroupsCacheTimeInMinutes"]);
                    _memoryCache.Set<string>(CACHE_KEY + userId, JsonConvert.SerializeObject(groups), DateTime.Now.AddHours(SubscriberGroupsCacheTimeInMinutes).TimeOfDay);               
                }      
                catch (Exception ex)
                {
                    _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_ApiGroupAuthorizationHandler.CheckAuthAsync Error getting user group info for user: {userId}  Exception: {ex.Message}", requirement);
                    // if exception then stop here, assume they don't have access
                    return false;
                }
            }

            // if no groups then just not authorized at this point
            if (groups.Count < 1)
            {
                _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_ApiGroupAuthorizationHandler.CheckAuthAsync No groups from for user: {userId}");
                return false;
            }

            _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync returning {groups.Contains(requirement.RoleName)} for user: {userId}");
            // get the configured groups
            return groups.Contains(requirement.RoleName);
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupRequirement requirement)
        {
            bool isAuth = await this.CheckAuthAsync(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requirement);
            if (isAuth)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class GroupRequirement : IAuthorizationRequirement
    {
        public string RoleName;
        public Claim claim;
        public GroupRequirement(string role)
        {
            RoleName = role;
            claim = new Claim("Role", RoleName);
        }
    }
}