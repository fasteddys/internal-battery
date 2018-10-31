using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;

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
            var redirectUrl = Url.Action(nameof(HomeController.Profile), "Home");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }


        [HttpGet]
        public IActionResult SignIn()
        {
            SetAzureAdB2CCulture();
            var redirectUrl = Url.Action(nameof(HomeController.Profile), "Home");        
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl},
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            SetAzureAdB2CCulture();
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
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
    }
}