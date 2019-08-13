using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UpDiddy.Models;
using Microsoft.Identity.Client;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace UpDiddy.Controllers
{
    public class SessionController : Controller
    {
        public SessionController(IOptions<AzureAdB2COptions> b2cOptions)
        {
            AzureAdB2COptions = b2cOptions.Value;
        }

        public AzureAdB2COptions AzureAdB2COptions { get; set; }


        [HttpGet]
        public IActionResult SignInAndEnroll()
        {
            SetAzureAdB2CCulture();
            var redirectUrl = Url.Action(nameof(HomeController.LoggingIn), "Home");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }


        [HttpGet]
        public IActionResult SignIn([FromQuery] string redirectUri)
        {
            SetAzureAdB2CCulture();
            /** 
             * Due to iOS issues, we need to introduce a landing page so that the call to
             * our profile page is coming from the same HTTPcontext session as our site.
             * */
            if(string.IsNullOrEmpty(redirectUri))
                redirectUri = Url.Action(nameof(HomeController.LoggingIn), "Home");

            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUri},
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            SetAzureAdB2CCulture();
            var redirectUrl = Url.Action(nameof(SessionController.SignOut), "Session");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = AzureAdB2COptions.ResetPasswordPolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            SetAzureAdB2CCulture();
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = AzureAdB2COptions.EditProfilePolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            SetAzureAdB2CCulture();
            var callbackUrl = Url.Action(nameof(SignedOut), "Session", values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return View();
        }


        private void  SetAzureAdB2CCulture()
        {
            // Get the current culture and assign it to Azure options so identity screens will be localized
            var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
            AzureAdB2COptions.UiLocales = requestCulture.RequestCulture.Culture.Name;
        }

        [HttpGet]
        [Authorize]
        [Route("[controller]/token")]
        public async Task<IActionResult> TokenAsync()
        {
            // Retrieve the token with the specified scopes
            // Retrieve the token with the specified scopes
            var scope = AzureAdB2COptions.ApiScopes.Split(' ');
            string signedInUserID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(AzureAdB2COptions.ClientId)
                .WithB2CAuthority(AzureAdB2COptions.Authority)
                .WithRedirectUri(AzureAdB2COptions.RedirectUri)
                .WithClientSecret(AzureAdB2COptions.ClientSecret)
                .Build();
            
            new MSALSessionCache(signedInUserID, HttpContext).EnablePersistence(app.UserTokenCache); 
            var accounts = await app.GetAccountsAsync();

            AuthenticationResult result = await app.AcquireTokenSilent(scope, accounts.FirstOrDefault()).ExecuteAsync();
            return Ok(new { accessToken = result.AccessToken, expiresOn = result.ExpiresOn, uniqueId = result.UniqueId });
        }
    }
}