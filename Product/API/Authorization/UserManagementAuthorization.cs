using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Shared;

namespace UpDiddyApi.Authorization
{
    public class UserManagementAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private IConfiguration _configuration;

        public UserManagementAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            string userManagementAuthorizationHeader = context.Request.Headers["UserManagement"];
            if (!string.IsNullOrEmpty(userManagementAuthorizationHeader))
            {
                string decryptedUserManagementAuthorization = Crypto.Decrypt(_configuration["Crypto:Key"], userManagementAuthorizationHeader);
                string userManagementAuthorization = _configuration["Auth0:ManagementApi:ClientSecret"];
                if (decryptedUserManagementAuthorization == userManagementAuthorization)
                {
                    await _next.Invoke(context);
                    return;
                }
            }

            //Reject request if there is no user authorization header or if it is not valid
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");

        }
    }

    public static class UserManagementAuthorizationMiddlewareExtension
    {
        public static IApplicationBuilder UseUserManagementAuthorization(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<UserManagementAuthorizationMiddleware>();
        }
    }

    public class UserManagementAuthorizationPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseUserManagementAuthorization();
        }
    }
}