using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services.AzureAPIManagement;

namespace UpDiddyApi.Authorization.APIGateway
{
    internal class APIGatewayAuthHandler : AuthenticationHandler<APIGatewayOptions>
    {
        private IAPIGateway _apiGateway;
        private IHttpContextAccessor _httpContextAccessor;
        private ILogger _logger;

        public APIGatewayAuthHandler(IOptionsMonitor<APIGatewayOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder, ISystemClock clock, IAPIGateway apiGateway, IHttpContextAccessor httpContextAccessor, ILogger<APIGatewayAuthHandler> logger) : base(options, loggerFactory, encoder, clock)
        {
            _apiGateway = apiGateway;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string key = _httpContextAccessor.HttpContext.Request.Headers["Ocp-Apim-Subscription-Key"];
            string subscriptionId = _httpContextAccessor.HttpContext.Request.Headers["subscription-id"];
            if (key == null || subscriptionId == null)
            {
                _logger.Log(LogLevel.Warning, $"APIGatewayAuthHandler.HandleAuthenticateAsync: Missing required headers, unable to authenticate to {APIGatewayDefaults.AuthenticationScheme}");
                return AuthenticateResult.Fail("Missing required headers: Ocp-Apim-Subscription-Key and/or subscription-id");
            }


            string userId = await _apiGateway.GetUserIdAsync(subscriptionId, key);
            if(userId == null)
            {
                _logger.Log(LogLevel.Warning, "APIGatewayAuthHandler.HandleAuthenticateAsync: Invalid key presented for subscriptionId: {@subscriptionId}", subscriptionId);
                return AuthenticateResult.Fail("Invalid token");
            }

            User user = await _apiGateway.GetUserAsync(userId);

            var claims = new[] { new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.NameIdentifier, user.GetUserId())};
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            _logger.Log(LogLevel.Information, "APIGatewayAuthHandler.HandleAuthenticateAsync: subscriptionId: {@subscriptionId} successfully authenticated.", subscriptionId);
            return AuthenticateResult.Success(ticket);
        }
    }
}