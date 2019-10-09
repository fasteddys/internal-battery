using System.Linq;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.ViewModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
namespace UpDiddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AuthenticationApiClient client = new AuthenticationApiClient(new Uri($"https://{_configuration["Auth0:Domain"]}/"));
                    string domain =  $"https://{_configuration["Auth0:Domain"]}";
                    var result = await client.GetTokenAsync(new ResourceOwnerTokenRequest
                    {
                        ClientId = _configuration["Auth0:ClientId"],
                        ClientSecret = _configuration["Auth0:ClientSecret"],
                        Scope = "openid profile email",
                        Realm = "Username-Password-Authentication", // Specify the correct name of your DB connection
                        Username = vm.EmailAddress,
                        Password = vm.Password,
                        Audience = _configuration["Auth0:Audience"]
                    });

                    var user = await client.GetUserInfoAsync(result.AccessToken);
                    var subscriberGuid = user.AdditionalClaims.Where(x => x.Key == ClaimTypes.NameIdentifier).FirstOrDefault().Value.ToString();
                    var expiresOn = DateTime.UtcNow.AddSeconds(result.ExpiresIn);
                    
                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, subscriberGuid));
                    claims.Add(new Claim(ClaimTypes.Name, user.FullName));
                    claims.Add(new Claim("access_token", result.AccessToken));
                    claims.Add(new Claim(ClaimTypes.Expiration, expiresOn.ToString()));

                    var permissionClaim = user.AdditionalClaims.Where(x => x.Key == ClaimTypes.Role).FirstOrDefault().Value.ToList();
                    string permissions = string.Empty;

                    foreach (var permission in permissionClaim)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, permission.ToString(),ClaimValueTypes.String,domain));
                    }

                    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));                 
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                    return RedirectToLocal(returnUrl);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }

            return View(vm);
        }




        [HttpGet]
        public async Task LoginExternal(string connection, string returnUrl = "/")
        {
            var properties = new AuthenticationProperties() { RedirectUri = returnUrl };

            if (!string.IsNullOrEmpty(connection))
                properties.Items.Add("connection", connection);

            await HttpContext.ChallengeAsync("Auth0", properties);
        }

        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be whitelisted in the
                // **Allowed Logout URLs** settings for the client.
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// This is just a helper action to enable you to easily see all claims related to a user. It helps when debugging your
        /// application to see the in claims populated from the Auth0 ID Token
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Claims()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
