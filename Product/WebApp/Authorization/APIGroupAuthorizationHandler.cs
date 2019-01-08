using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
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
        IDistributedCache _cache;
        IApi _api;

        public ApiGroupAuthorizationHandler(IApi api, IConfiguration configuration, IDistributedCache distributedCache)
        {
            _api = api;
            _configuration = configuration;
            _cache = distributedCache;
        }

        private async Task<bool> CheckAuthAsync(string userId, GroupRequirement requirement)
        {
            IList<string> groups = new List<string>();
            try
            {
                SubscriberADGroupsDto dto = _api.MyGroups();
                groups = dto.groups;
            }
            catch (Exception e)
            {
                // if exception then stop here, assume they don't have access
                return false;
            }

            // if no groups then just not authorized at this point
            if (groups.Count < 1)
                return false;

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