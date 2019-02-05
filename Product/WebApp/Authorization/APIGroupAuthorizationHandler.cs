using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
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

        public const string CACHE_KEY = "SubscriberGroups";

        public ApiGroupAuthorizationHandler(IApi api, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _api = api;
            _configuration = configuration;
            _accessor = contextAccessor;
        }

        private async Task<bool> CheckAuthAsync(string userId, GroupRequirement requirement)
        {
            IList<string> groups = new List<string>();
            string cachedGroups = _accessor.HttpContext.Session.GetString(CACHE_KEY + userId);
            if(cachedGroups != null)
            {
                groups = JsonConvert.DeserializeObject<List<string>>(cachedGroups);
            } else
            {
                try
                {
                    SubscriberADGroupsDto dto = await _api.MyGroupsAsync();
                    groups = dto.groups;
                    _accessor.HttpContext.Session.SetString(CACHE_KEY + userId, JsonConvert.SerializeObject(groups));
                }
                catch (Exception e)
                {
                    // if exception then stop here, assume they don't have access
                    return false;
                }
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