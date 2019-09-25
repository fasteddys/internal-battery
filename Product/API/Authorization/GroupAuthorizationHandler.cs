using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;

namespace UpDiddyApi.Authorization
{
    public class GroupAuthorizationHandler : AuthorizationHandler<GroupRequirement>
    {
        IB2CGraph _graphClient;
        IConfiguration _configuration; 
        ILogger _syslog = null;
        IMemoryCache _memoryCache = null;


        public GroupAuthorizationHandler(IB2CGraph graphClient, IConfiguration configuration, ILogger<GroupAuthorizationHandler> sysLog, IMemoryCache memoryCache)
        {
            _graphClient = graphClient;
            _configuration = configuration; 
            _syslog = sysLog;
            _memoryCache = memoryCache;
        }


        private async Task<bool> CheckAuthAsyncSave(string userId, GroupRequirement requirement)
        {

        

            // important to not use "SubscriberGroups" as the cache key since that what the webapp is using and the cached type is different
            string cacheKey = "APISubscriberGroups" + userId;
            string cachedGroups = _memoryCache.Get<String>(cacheKey);

            IList<Microsoft.Graph.Group> groups = null;
            if (cachedGroups != null)
            {
                groups = JsonConvert.DeserializeObject<IList<Microsoft.Graph.Group>>(cachedGroups);
            }
            else
            {
                groups = new List<Microsoft.Graph.Group>();
                try
                {
                    groups = await _graphClient.GetUserGroupsByObjectId(userId);
                    int SubscriberGroupsCacheTimeInMinutes = int.Parse(_configuration["CareerCircle:SubscriberGroupsCacheTimeInMinutes"]);
                    _memoryCache.Set<String>(cacheKey, JsonConvert.SerializeObject(groups), DateTime.Now.AddHours(SubscriberGroupsCacheTimeInMinutes).TimeOfDay);
                }
                catch (Exception ex)
                {
                    _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync Error getting user group info for user: {userId}  Exception: {ex.Message}", requirement);
                    // if exception then stop here, don't assume they have access
                    return false;
                }

                // if no groups then just not authorized at this point
                if (groups.Count < 1)
                {
                    _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync No groups from for user: {userId}");
                    return false;
                }
            }

            // get the configured groups
            List<ConfigADGroup> requiredGroups = _configuration.GetSection("ADGroups:Values")
                .Get<List<ConfigADGroup>>()
                .Where(e => requirement.Claims.Where(c => c.Value == e.Name).Any())
                .ToList();

            // claims in GroupRequirement are treated as OR conditions
            Microsoft.Graph.Group group = groups.Where(e => requiredGroups.Where(rg => rg.Id.Equals(e.AdditionalData["objectId"])).Any()).FirstOrDefault();

            _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync returning {group != null} for user: {userId}");

            return group != null;
        }
        private async Task<bool> CheckAuthAsync(string userId, GroupRequirement requirement)
        {

            // Getting a weird error ocsasionally when logging.  See NOTE below 
            bool IsLogging = false; ;
            try
            {
                // important to not use "SubscriberGroups" as the cache key since that what the webapp is using and the cached type is different
                string cacheKey = "APISubscriberGroups" + userId;
                string cachedGroups = _memoryCache.Get<String>(cacheKey);

                IList<Microsoft.Graph.Group> groups = null;
                if (cachedGroups != null)
                {
                    groups = JsonConvert.DeserializeObject<IList<Microsoft.Graph.Group>>(cachedGroups);
                }
                else
                {
                    groups = new List<Microsoft.Graph.Group>();

                    try
                    {
                        groups = await _graphClient.GetUserGroupsByObjectId(userId);
                        int SubscriberGroupsCacheTimeInMinutes = int.Parse(_configuration["CareerCircle:SubscriberGroupsCacheTimeInMinutes"]);
                        _memoryCache.Set<String>(cacheKey, JsonConvert.SerializeObject(groups), DateTime.Now.AddHours(SubscriberGroupsCacheTimeInMinutes).TimeOfDay);
                    }
                    catch (Exception ex)
                    {

                        IsLogging = true;

                        /*
                         NOTE: Sometimes the graph call returns the following error:

                            { System.Net.WebException: Error Calling the Graph API:
                                {
                                    "odata.error": {
                                        "code": "Request_ResourceNotFound",
                                        "message": {
                                        "lang": "en",
                                        "value": "Resource '11f36619-ea54-4c56-a365-8cec7bac5d65' does not exist or one of its queried reference-property objects are not present."
                                        },
                                        "requestId": "070e7516-b38f-48c0-8712-d1b0d1f4fa36",
                                        "date": "2019-09-25T12:26:21"
                                }
                            }


                            This error seems to be due to a time delay with a newly created subscriber not being found in graph.  This error was discovered during the development of the 
                            viral recruitment enhancments for OWL  The flow that causes this error is:

                            Click Viral Link -> View Job -> Click Apply for job -> Create new account -> View Job Application screen (which results in this error)

                            When the graph call returns this error, the log exception below throws an "Invalid Format" exception.
                            */

                            _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync Error getting user group info for user: {userId}  Exception: {ex.Message}", requirement);
                        // if exception then stop here, don't assume they have access
                        return false;
                    }

                    // if no groups then just not authorized at this point
                    if (groups.Count < 1)
                    {       
                        IsLogging = true;
                        _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync No groups from for user: {userId}");
                        return false;
                    }
                }

                // get the configured groups
                List<ConfigADGroup> requiredGroups = _configuration.GetSection("ADGroups:Values")
                    .Get<List<ConfigADGroup>>()
                    .Where(e => requirement.Claims.Where(c => c.Value == e.Name).Any())
                    .ToList();

                // claims in GroupRequirement are treated as OR conditions
                Microsoft.Graph.Group group = groups.Where(e => requiredGroups.Where(rg => rg.Id.Equals(e.AdditionalData["objectId"])).Any()).FirstOrDefault();
                IsLogging = true;
                _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync returning {group != null} for user: {userId}");
                return group != null;

            }
            catch (Exception ex)
            {
                var info = ex.Message;
                // Work around for intermitten syslog.Log exception.  See NOTE above
                if (IsLogging)
                    return false;
                else
                    throw ex;
            }

        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupRequirement requirement)
        {
            bool isAuth = await this.CheckAuthAsync(context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, requirement);
            if (isAuth)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class GroupRequirement : IAuthorizationRequirement
    {
        public string[] RoleNames;
        public List<Claim> Claims;
        public GroupRequirement(string[] roles)
        {
            RoleNames = roles;
            Claims = new List<Claim>();
            foreach(string role in roles)
            {
                Claims.Add(new Claim(role, role));
            }
        }
    }
}