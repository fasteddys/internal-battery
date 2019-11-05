using System;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UpDiddy.Authorization
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Issuer == requirement.Issuer))
                return Task.CompletedTask;

            var scopes = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            
            if (scopes.Any(x => requirement.Claims.Any(y => string.Equals(x.Value, y.Value, StringComparison.OrdinalIgnoreCase))))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public string[] RoleNames;
        public string Issuer;
        public List<Claim> Claims;
        public HasScopeRequirement(string[] roles, string issuer)
        {
            RoleNames = roles;
            Issuer = issuer;
            Claims = new List<Claim>();
            foreach (string role in roles)
            {
                Claims.Add(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, issuer));
            }
        }
    }
}
