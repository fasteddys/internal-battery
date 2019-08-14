using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        IDistributedCache _cache;
        ILogger _syslog = null;
 

        public GroupAuthorizationHandler(IB2CGraph graphClient, IConfiguration configuration, IDistributedCache distributedCache, ILogger<GroupAuthorizationHandler> sysLog)
        {
            _graphClient = graphClient;
            _configuration = configuration;
            _cache = distributedCache;
            _syslog = sysLog;
        }

        private async Task<bool> CheckAuthAsync(string userId, GroupRequirement requirement)
        {
            IList<Microsoft.Graph.Group> groups = new List<Microsoft.Graph.Group>();
            try
            {
                groups = await _graphClient.GetUserGroupsByObjectId(userId);
            }
            catch (Exception ex)
            {
                _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync Error getting user group info for user: {userId}  Exception: {ex.Message}" , requirement);
                // if exception then stop here, don't assume they have access
                return false;
            }

            // if no groups then just not authorized at this point
            if (groups.Count < 1)
            {
                _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync No groups from for user: {userId}" );
                return false;
            }
                

            // get the configured groups
            List<ConfigADGroup> requiredGroups = _configuration.GetSection("ADGroups:Values")
                .Get<List<ConfigADGroup>>()
                .Where(e => requirement.Claims.Where(c => c.Value ==e.Name).Any())
                .ToList();

            // claims in GroupRequirement are treated as OR conditions
            Microsoft.Graph.Group group = groups.Where(e => requiredGroups.Where(rg => rg.Id.Equals(e.AdditionalData["objectId"])).Any()).FirstOrDefault();

            _syslog.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"MSAL_GroupAuthorizationHandler.CheckAuthAsync returning {group != null} for user: {userId}");
            return group != null;
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