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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using UpDiddy.ViewModels;
using Auth0.AuthenticationApi;
using Microsoft.Extensions.Configuration;
using Auth0.AuthenticationApi.Models;
using System.Collections.Generic;
using UpDiddy.Api;
using UpDiddyLib.Dto.User;

namespace UpDiddy.Controllers
{
    public class SessionController : Controller
    {
        private ILogger _syslog = null;
        private IDistributedCache _cache = null;
        private IMemoryCache _memoryCache = null;
        private IConfiguration _configuration = null;
        private IApi _api = null;

        public SessionController(ILogger<SessionController> sysLog, IDistributedCache distributedCache, IMemoryCache memoryCache, IConfiguration configuration, IApi api)
        {
            _syslog = sysLog;
            _cache = distributedCache;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _api = api;
        }

        [HttpGet]
        public IActionResult SignUp(string returnUrl = "/")
        {
            return View(new SignUpViewModel()
            {
                IsExpressSignUp = true
            });
        }

        [HttpGet]
        public IActionResult SignIn(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel vm, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isUserExistsInAuth0 = await _api.IsUserExistsInAuth0Async(vm.EmailAddress);
                    if (isUserExistsInAuth0)
                    {
                        // perform login attempt using Auth0
                        AuthenticationApiClient client = new AuthenticationApiClient(new Uri($"https://{_configuration["Auth0:Domain"]}/"));
                        string domain = $"https://{_configuration["Auth0:Domain"]}";
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
                        var firstClaimValue = user.AdditionalClaims.Where(x => x.Key == ClaimTypes.NameIdentifier).FirstOrDefault();
                        Guid subscriberGuid;
                        if (firstClaimValue.Value != null && Guid.TryParse(firstClaimValue.Value.ToString(), out subscriberGuid))
                        {
                            var expiresOn = DateTime.UtcNow.AddSeconds(result.ExpiresIn);

                            List<Claim> claims = new List<Claim>();
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, subscriberGuid.ToString()));
                            claims.Add(new Claim(ClaimTypes.Name, user.FullName));
                            claims.Add(new Claim("access_token", result.AccessToken));
                            claims.Add(new Claim(ClaimTypes.Expiration, expiresOn.ToString()));

                            var permissionClaim = user.AdditionalClaims.Where(x => x.Key == ClaimTypes.Role).FirstOrDefault().Value.ToList();
                            string permissions = string.Empty;

                            foreach (var permission in permissionClaim)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, permission.ToString(), ClaimValueTypes.String, domain));
                            }

                            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                            // sync Auth0's email verification status with ours. it would be better if this behavior could be triggered outside of the front-end code,
                            // but having difficulty finding a way to trigger that when the data changes (not just when a login occurs): https://community.auth0.com/t/unable-to-send-context-accesstoken/32098
                            if (user.EmailVerified.HasValue)
                                await _api.UpdateEmailVerificationStatusAsync(subscriberGuid, user.EmailVerified.Value);

                            return RedirectToLocal(returnUrl);
                        }
                        else
                        {
                            throw new ApplicationException("Unable to identify the subscriber.");
                        }
                    }
                    else
                    {
                        var isUserExistsInADB2C = await _api.IsUserExistsInADB2CAsync(vm.EmailAddress);
                        if (isUserExistsInADB2C)
                        {
                            // attempt login to adb2c with user's credentials.
                            bool isADB2CLoginValid = await _api.CheckADB2CLoginAsync(vm.EmailAddress, vm.Password);
                                                        
                            if (isADB2CLoginValid)
                            {
                                // perform the user migration, mark as verified if the adb2c user is verified email address
                                var response = await _api.MigrateUserAsync(new CreateUserDto() { Email = vm.EmailAddress, Password = vm.Password });   

                            }
                            else
                            {
                                ModelState.AddModelError("", "Invalid username or password.");
                            }
                        }
                        else
                        {
                            // no account found
                            ModelState.AddModelError("", "No account found with that email address.");
                        }
                    }


                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            throw new NotImplementedException();
        }


        [HttpGet]
        public async Task SignOut()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("[controller]/token")]
        public IActionResult TokenAsync()
        {
            var subscriberGuid = HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value.ToString();
            var accessToken = HttpContext.User.Claims.Where(x => x.Type == "access_token").FirstOrDefault().Value.ToString();
            var expiresOn = HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Expiration).FirstOrDefault().Value.ToString();
            return Ok(new { accessToken = accessToken, expiresOn = expiresOn, uniqueId = subscriberGuid });
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