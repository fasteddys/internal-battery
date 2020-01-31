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
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

 
namespace UpDiddy.Controllers
{
    public class SessionController : Controller
    {
        private ILogger _syslog = null;
        private IDistributedCache _cache = null;
        private IMemoryCache _memoryCache = null;
        private IApi _api = null;
        private AuthenticationApiClient _auth0Client = null;
        private string _auth0Domain = null;
        private string _auth0ClientId = null;
        private string _auth0ClientSecret = null;
        private string _auth0Audience = null;
        private string _auth0Connection = null;

        public SessionController(ILogger<SessionController> sysLog, IDistributedCache distributedCache, IMemoryCache memoryCache, IConfiguration configuration, IApi api)
        {
            _syslog = sysLog;
            _cache = distributedCache;
            _memoryCache = memoryCache;
            _api = api;
            _auth0Client = new AuthenticationApiClient(new Uri($"https://{configuration["Auth0:Domain"]}/"));
            _auth0Domain = $"https://{configuration["Auth0:Domain"]}";
            _auth0ClientId = configuration["Auth0:ClientId"];
            _auth0ClientSecret = configuration["Auth0:ClientSecret"];
            _auth0Audience = configuration["Auth0:Audience"];
            _auth0Connection = configuration["Auth0:Connection"];
        }

        [HttpGet]
        [Route("/session/signup")]
        public IActionResult SignUp(string returnUrl = "/")
        {
            return View(new SignUpViewModel());
        }

        [HttpPost]
        [Route("/session/signup")]
        public async Task<IActionResult> SignUp(SignUpViewModel vm, [FromQuery] string returnUrl = "/session/signin")
        {
            bool modelHasAllFields = !string.IsNullOrEmpty(vm.Email) &&
                !string.IsNullOrEmpty(vm.Password) &&
                !string.IsNullOrEmpty(vm.ReenterPassword);

            if (vm.IsWaitList)
            {
                modelHasAllFields = !string.IsNullOrEmpty(vm.FirstName) &&
                !string.IsNullOrEmpty(vm.LastName);
            }

            if (!modelHasAllFields)
            {

                _syslog.LogError($"SessionController:SignUp  Please enter all sign-up fields and try again.");
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Please enter all sign-up fields and try again."
                });
            }

            if (!ModelState.IsValid)
            {

                _syslog.LogError("SessionController:SignUp Unfortunately, an error has occured with your submission.Please try again.");
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Unfortunately, an error has occured with your submission. Please try again."
                });
            }

            if (!vm.Password.Equals(vm.ReenterPassword))
            {
                _syslog.LogError("SessionController:SignUp User's passwords do not match.");
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 403,
                    Description = "User's passwords do not match."
                });
            }

            CreateUserDto createUserDto = new CreateUserDto
            {
                Email = vm.Email,
                Password = vm.Password,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                PhoneNumber = vm.PhoneNumber,
                ReferrerUrl = Request.Headers["Referer"].ToString(),
                JobReferralCode = Request.Cookies["referrerCode"] == null ? null : Request.Cookies["referrerCode"].ToString(),
                PartnerGuid = vm.PartnerGuid,
                IsAgreeToMarketingEmails = vm.IsAgreeToMarketingEmails,
                IsGatedDownload = vm.IsGatedDownload,
                GatedDownloadFileUrl = vm.GatedDownloadFileUrl,
                GatedDownloadMaxAttemptsAllowed = vm.GatedFileDownloadMaxAttemptsAllowed
            };

            try
            {
                BasicResponseDto basicResponseDto = await _api.CreateUserAsync(createUserDto);

                if (basicResponseDto.StatusCode == 200)
                {
                    return Ok(basicResponseDto);
                }
                else
                {
                    _syslog.LogError($"SessionController:SignUp CreateUser returned statusCode = {basicResponseDto.StatusCode} Description = { basicResponseDto.Description}");
                    return StatusCode(500, new BasicResponseDto()
                    {
                        StatusCode = 500,
                        Description = basicResponseDto.Description
                    });
                }
            }
            catch (ApiException e)
            {
                _syslog.LogError($"SessionController:SignUp CreateUser returned Error = {e.ResponseDto.StatusCode} Description = { e.ResponseDto.Description} EMail = {createUserDto.Email}");
                return StatusCode(500, new BasicResponseDto
                {
                    StatusCode = 500,
                    Description = e.ResponseDto.Description
                });
            }
        }

        [HttpPost]
        [Route("/session/existing-user-sign-up")]
        public async Task<IActionResult> ExistingUserSignUp(SignUpViewModel vm, [FromQuery] string returnUrl = "/session/signin")
        {
            bool modelHasAllFields = !string.IsNullOrEmpty(vm.Email);

            if (vm.IsWaitList)
                modelHasAllFields = !string.IsNullOrEmpty(vm.FirstName) && !string.IsNullOrEmpty(vm.LastName);

            if (!modelHasAllFields)
            {
                _syslog.LogError($"SessionController:ExistingUserSignUp Please enter all sign-up fields and try again.");
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Please enter all sign-up fields and try again."
                });
            }

            CreateUserDto createUserDto = new CreateUserDto
            {
                SubscriberGuid = vm.SubscriberGuid.Value,
                Email = vm.Email,
                FirstName = vm.IsWaitList ? vm.FirstName : null,
                LastName = vm.IsWaitList ? vm.LastName : null,
                PhoneNumber = vm.IsWaitList ? vm.PhoneNumber : null,
                ReferrerUrl = Request.Headers["Referer"].ToString(),
                JobReferralCode = Request.Cookies["referrerCode"] == null ? null : Request.Cookies["referrerCode"].ToString(),
                PartnerGuid = vm.PartnerGuid,
                IsGatedDownload = vm.IsGatedDownload,
                GatedDownloadFileUrl = vm.GatedDownloadFileUrl,
                GatedDownloadMaxAttemptsAllowed = vm.GatedFileDownloadMaxAttemptsAllowed
            };

            try
            {
                BasicResponseDto basicResponseDto = await _api.ExistingUserSignup(createUserDto);

                if (basicResponseDto.StatusCode == 200)
                {
                    return Ok(basicResponseDto);
                }
                else
                {
                    return StatusCode(400, new BasicResponseDto()
                    {
                        StatusCode = 400,
                        Description = basicResponseDto.Description
                    });
                }
            }
            catch (ApiException e)
            {
                return StatusCode(500, new BasicResponseDto
                {
                    StatusCode = 500,
                    Description = e.ResponseDto.Description
                });
            }
        }

        [HttpGet]
        [Route("/session/signin")]
        public IActionResult SignIn(string returnUrl = "/Talent/Subscribers")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("/session/signin")]
        public async Task<IActionResult> SignIn(SignInViewModel vm, [FromQuery] string returnUrl = "/Talent/Subscribers")
        {
            int step = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    var isUserExistsInAuth0 = await _api.IsUserExistsInAuth0Async(vm.EmailAddress);
                    if (isUserExistsInAuth0)
                    {
                        step = 1;
                        _syslog.LogInformation($"SessionController:SignIn Is existing user in Auth0");
                        await ExecuteAuth0SignInAsync(vm.EmailAddress, vm.Password);
                        step = 2;
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        step = 3;
                        var isUserExistsInADB2C = await _api.IsUserExistsInADB2CAsync(vm.EmailAddress);
                        step = 4;
                        if (isUserExistsInADB2C)
                        {
                            step = 5;
                            // attempt login to adb2c with user's credentials.
                            bool isADB2CLoginValid = await _api.CheckADB2CLoginAsync(vm.EmailAddress, vm.Password);

                            if (isADB2CLoginValid)
                            {
                                step = 6;
                                // perform the user migration
                                var isMigrationSuccessful = await _api.MigrateUserAsync(new CreateUserDto() { Email = vm.EmailAddress, Password = vm.Password });
                                step = 7;
                                if (isMigrationSuccessful)
                                {
                                    step = 8;
                                    // log the user in using their newly created auth0 account
                                    await ExecuteAuth0SignInAsync(vm.EmailAddress, vm.Password);
                                    step = 9;
                                    return RedirectToLocal(returnUrl);
                                }
                                else
                                {
                                    step = 10;
                                    _syslog.LogError($"SessionController:SignIn An internal error occurred.");
                                    // intentionally being vague about the error
                                    ModelState.AddModelError("", "An internal error occurred.");
                                }
                            }
                            else
                            {
                                step = 11;
                                _syslog.LogError($"SessionController:SignIn Invalid username or password.");
                                ModelState.AddModelError("", "Invalid username or password.");
                            }
                        }
                        else
                        {
                            step = 12;
                            // no account found
                            _syslog.LogError($"SessionController:SignIn No account found with that email address.");
                            ModelState.AddModelError("", "No account found with that email address.");
                        }
                    }
                }
                catch (Auth0.Core.Exceptions.ApiException ae)
                {
                    _syslog.LogError($"SessionController:SignIn Auth0 core exception Message = {ae.Message} step = {step}");
                    ModelState.AddModelError("", ae.Message);
                }
                catch (Exception e)
                {
                    _syslog.LogError($"SessionController:SignIn  An unexpected error occurred in SessionController.SignIn.  Message =  {e.Message} step = {step}", e);
                    ModelState.AddModelError("", "An unexpected error occurred.");
                }
            }
            else
                _syslog.LogError($"SessionController:SignIn  Model state is not valid");

            return View(vm);
        }

        [HttpGet]
        [Route("/session/resetpassword")]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpGet]
        [Route("/session/changepassword/{passwordResetRequestGuid}")]
        public async Task<IActionResult> ChangePassword(Guid passwordResetRequestGuid)
        {
            var isPasswordResetRequestValid = await _api.CheckValidityOfPasswordResetRequest(passwordResetRequestGuid);

            if (!isPasswordResetRequestValid)
                return RedirectToAction(nameof(SessionController.ResetPassword), new { success = "false", message = "This password request is not valid." });
            else
                return View(new ChangePasswordViewModel() { PasswordResetRequestGuid = passwordResetRequestGuid });
        }

        [HttpPost]
        [Route("/session/submitpassword")]
        public async Task<IActionResult> SubmitPassword(ChangePasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var changePasswordResult = await _api.ConsumeCustomPasswordResetAsync(vm.PasswordResetRequestGuid, vm.Password);

                if (changePasswordResult)
                    return RedirectToAction(nameof(SessionController.SignIn), new { success = "true", message = "Your password has been changed." });
                else
                {
                    _syslog.LogError($"SessionController:SubmitPassword There was a problem resetting the password.");
                    ModelState.AddModelError("CustomPasswordResetError", "There was a problem resetting the password.");
                    return View(nameof(SessionController.ChangePassword), vm);
                }
            }
            else
                return View(nameof(SessionController.ChangePassword), vm);
        }

        [HttpPost("/session/resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isUserExistsInAuth0 = await _api.IsUserExistsInAuth0Async(vm.EmailAddress);
                    if (!isUserExistsInAuth0)
                    {
                        var isUserExistsInADB2C = await _api.IsUserExistsInADB2CAsync(vm.EmailAddress);

                        if (isUserExistsInADB2C)
                        {
                            // perform the migration with a random password
                            var randomPassword = Utils.GeneratePassword(true, true, true, true, 20);
                            var isMigrationSuccessful = await _api.MigrateUserAsync(new CreateUserDto() { Email = vm.EmailAddress, Password = randomPassword });
                            if (!isMigrationSuccessful)
                            {
                                _syslog.LogError($"SessionController:ResetPassword There was a problem reseting your password.");
                                return BadRequest(new BasicResponseDto
                                {
                                    StatusCode = 400,
                                    Description = "There was a problem reseting your password."
                                });
                            }
                        }
                        else
                        {
                            _syslog.LogError($"SessionController:ResetPassword Email address is not recognized.");
                            // user doesn't exist in either environment
                            return BadRequest(new BasicResponseDto
                            {
                                StatusCode = 400,
                                Description = "Email address is not recognized."
                            });
                        }
                    }

                    var isPasswordResetInitiatedSuccessfully = await _api.CreateCustomPasswordResetAsync(vm.EmailAddress);
                    if (isPasswordResetInitiatedSuccessfully)
                        return Ok(new BasicResponseDto
                        {
                            StatusCode = 200,
                            Description = "Password reset has been initiated."
                        });
                    else
                    {
                        _syslog.LogError($"SessionController:ResetPassword There was a problem initating the password reset.");
                        return BadRequest(new BasicResponseDto
                        {
                            StatusCode = 400,
                            Description = "There was a problem initating the password reset."
                        });
                    }
                }
                catch (Auth0.Core.Exceptions.ApiException ae)
                {
                    _syslog.LogError($"SessionController:ResetPassword There was a problem resetting a user's password: {ae.Message}", ae);
                    return BadRequest(new BasicResponseDto
                    {
                        StatusCode = 400,
                        Description = "There was a problem resetting your password."
                    });
                }
                catch (Exception e)
                {
                    _syslog.LogError($"SessionController:ResetPassword An unexpected error occurred in SessionController.ResetPassword: {e.Message}", e);
                    return BadRequest(new BasicResponseDto
                    {
                        StatusCode = 400,
                        Description = "An unexpected error occurred."
                    });
                }
            }

            return View(vm);
        }

        [HttpGet]
        [Route("/session/signout")]
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

        private async Task ExecuteAuth0SignInAsync(string email, string password)
        {
            var result = await _auth0Client.GetTokenAsync(new ResourceOwnerTokenRequest
            {
                ClientId = _auth0ClientId,
                ClientSecret = _auth0ClientSecret,
                Scope = "openid profile email",
                Realm = _auth0Connection,
                Username = email,
                Password = password,
                Audience = _auth0Audience
            });

            var user = await _auth0Client.GetUserInfoAsync(result.AccessToken);
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
                    claims.Add(new Claim(ClaimTypes.Role, permission.ToString(), ClaimValueTypes.String, _auth0Domain));
                }

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                await _api.UpdateLastSignIn(subscriberGuid);
            }
            else
            {
                _syslog.LogError($"SessionController:ExecuteAuth0SignInAsync Unable to identify the subscriber.");
                throw new ApplicationException("Unable to identify the subscriber.");
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(TalentController.Subscribers), "Talent");
            }
        }

        #endregion
    }
}