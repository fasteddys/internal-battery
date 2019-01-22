using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
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

        public GroupAuthorizationHandler(IB2CGraph graphClient, IConfiguration configuration, IDistributedCache distributedCache)
        {
            _graphClient = graphClient;
            _configuration = configuration;
            _cache = distributedCache;
        }

        private async Task<bool> CheckAuthAsync(string userId, GroupRequirement requirement)
        {
            IList<Microsoft.Graph.Group> groups = new List<Microsoft.Graph.Group>();
            try
            {
                groups = await _graphClient.GetUserGroupsByObjectId(userId);
            }
            catch (Exception e)
            {
                // if exception then stop here, don't assume they have access
                return false;
            }

            // if no groups then just not authorized at this point
            if (groups.Count < 1)
                return false;

            // get the configured groups
            ConfigADGroup requiredGroup = _configuration.GetSection("ADGroups:Values")
                .Get<List<ConfigADGroup>>()
                .Find(e => e.Name == requirement.RoleName);

            Microsoft.Graph.Group group = groups.Where(e => e.Id == requiredGroup.Id).First();

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
        public string RoleName;
        public Claim claim;
        public GroupRequirement(string role)
        {
            RoleName = role;
            claim = new Claim("Role", RoleName);
        }
    }
}