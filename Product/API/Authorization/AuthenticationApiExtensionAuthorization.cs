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
    public class AuthenticationApiExtensionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private IConfiguration _configuration;

        public AuthenticationApiExtensionAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            string authenticationApiExtensionAuthorizationHeader = context.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authenticationApiExtensionAuthorizationHeader))            {
                string authenticationApiExtensionAuthorization = _configuration["Auth0:Extensions:AuthorizationHeader"];
                if (authenticationApiExtensionAuthorizationHeader == authenticationApiExtensionAuthorization)
                {
                    await _next.Invoke(context);
                    return;
                }
            }

            //Reject request if there is no authorization header or if it is not valid
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
        }
    }

    public static class AuthenticationApiExtensionAuthorizationMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthenticationApiExtensionAuthorization(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<AuthenticationApiExtensionAuthorizationMiddleware>();
        }
    }

    public class AuthenticationApiExtensionAuthorizationPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseAuthenticationApiExtensionAuthorization();
        }
    }
}