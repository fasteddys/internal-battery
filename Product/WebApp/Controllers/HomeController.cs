using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;
using System.Security.Claims;
using UpDiddy.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using UpDiddy.Helpers;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Polly.Registry;
using Microsoft.Extensions.Caching.Distributed;

namespace UpDiddy.Controllers
{
    public class HomeController : BaseController
    {
        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _HttpClientFactory;
        private readonly IHostingEnvironment _env;

        [HttpGet]
        public IActionResult GetCountries()
        {
            return Ok(Json(API.GetCountries()));
        }

        public HomeController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<HomeController> localizer, IConfiguration configuration, IHostingEnvironment env, IHttpClientFactory httpClientFactory, IDistributedCache cache)
            : base(azureAdB2COptions.Value, configuration, httpClientFactory, cache)        {
            _env = env;
            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
            _HttpClientFactory = httpClientFactory;
        }
        [HttpGet]
        public IActionResult GetStatesByCountry(Guid countryGuid)
        {
            return Ok(Json(API.GetStatesByCountry(countryGuid)));
        }
        
        public IActionResult Index()
        {
            // TODO remove test code 
            GetSubscriber(false);

            HomeViewModel HomeViewModel = new HomeViewModel(_configuration, API.Topics());
            return View(HomeViewModel);
        }

        public IActionResult TermsOfService()
        {
            return View();
        }

        public IActionResult News()
        {

            return View();
        }


        public IActionResult AboutUs()
        {

            return View();
        }


        public IActionResult Privacy()
        {

            return View();
        }


        public IActionResult WhatWeAreAbout()
        {

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }


        [Authorize]
        public IActionResult ProfileLogin()
        {
            GetSubscriber(true);
            // UPdated the subscribers course progress 
            if (this.subscriber != null)
                API.UpdateStudentCourseProgress((Guid) this.subscriber.SubscriberGuid, true);
 
            return RedirectToAction("Profile", "Home");
        }



        [Authorize]
        public IActionResult Profile()
        {
            GetSubscriber(true);
            IList<EnrollmentDto> CurrentEnrollments = API.GetCurrentEnrollmentsForSubscriber(this.subscriber);
            CountryDto SubscriberCountry = new CountryDto();
            StateDto SubscriberState = new StateDto();
            if (this.subscriber.StateId != 0)
            {
                // JAB: TODO explore "Query Types" as a means to get all of the subscriber information from a view
                //  https://docs.microsoft.com/en-us/ef/core/modeling/query-types                
                SubscriberCountry = API.GetSubscriberCountry(this.subscriber.StateId);
                SubscriberState = API.GetSubscriberState(this.subscriber.StateId);
            }
            IList<WozCourseProgress> WozCourseProgressions = new List<WozCourseProgress>();
          
            if ( CurrentEnrollments != null )
            {
            
                foreach (EnrollmentDto enrollment in CurrentEnrollments)
                {                   
                    try
                    {
                        Guid CourseGuid = enrollment.Course.CourseGuid ?? default(Guid);
                        Guid VendorGuid = enrollment.Course.Vendor.VendorGuid ?? default(Guid);
                        Guid SubscriberGuid = this.subscriber.SubscriberGuid?? default(Guid);
                        var courseLogin = API.CourseLogin(SubscriberGuid, CourseGuid,VendorGuid);

                        WozCourseProgress dto = new WozCourseProgress
                        {
                            CourseName = enrollment.Course.Name,
                            CourseUrl = courseLogin == null ? string.Empty : courseLogin.LoginUrl,
                            PercentComplete = enrollment.PercentComplete,
                            EnrollmentStatusId = enrollment.EnrollmentStatusId,
                            DisplayState = enrollment.EnrollmentStatusId
                        };

                        WozCourseProgressions.Add(dto);
                    }
                    catch (Exception e)
                    {
                        // Wire up logging for controller exceptions.
                    }
                }
            }
            
            ProfileViewModel ProfileViewModel = new ProfileViewModel(
                _configuration,
                this.subscriber,
                WozCourseProgressions,
                SubscriberCountry,
                SubscriberState);

            ProfileViewModel.Countries = API.GetCountries().Select(c => new SelectListItem()
            {
                Text = c.DisplayName,
                Value = c.CountryGuid.ToString()
            });

            ProfileViewModel.States = new List<StateViewModel>().Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.StateGuid.ToString()
            });
            ProfileViewModel.SelectedState = SubscriberState.StateGuid.HasValue ? SubscriberState.StateGuid.Value : Guid.Empty;
            ProfileViewModel.SelectedCountry = SubscriberCountry.CountryGuid;

            return View(ProfileViewModel);
        }

        [HttpPost]
        public BasicResponseDto UpdateProfileInformation(
            string UpdatedFirstName,
            string UpdatedLastName,
            string UpdatedAddress,
            string UpdatedPhoneNumber,
            string UpdatedCity,
            string UpdatedFacebookUrl,
            string UpdatedTwitterUrl,
            string UpdatedLinkedInUrl,
            string UpdatedStackOverflowUrl,
            string UpdatedGithubUrl,
            int UpdatedState,
            Guid CurrentSubscriberGuid,
            Guid SelectedState
            )
        {
            /* todo: the validation and sanitization logic below is probably the worst code i have written in the last 10 years. 
             * for the love of god, refactor this to use the following pattern: use IActionResult instead of BasicResponseDto for
             * return type, use data annotations for validation, simplify view model and pass that to method as a single parameter */
            this.GetSubscriber(false);

            // scrub fields which do not have strict validation to ensure no html is stored in the db
            if (UpdatedFirstName != null)
                UpdatedFirstName = WebUtility.HtmlDecode(Regex.Replace(UpdatedFirstName, "<[^>]*(>|$)", string.Empty));
            if (UpdatedLastName != null)
                UpdatedLastName = WebUtility.HtmlDecode(Regex.Replace(UpdatedLastName, "<[^>]*(>|$)", string.Empty));
            if (UpdatedAddress != null)
                UpdatedAddress = WebUtility.HtmlDecode(Regex.Replace(UpdatedAddress, "<[^>]*(>|$)", string.Empty));
            if (UpdatedCity != null)
                UpdatedCity = WebUtility.HtmlDecode(Regex.Replace(UpdatedCity, "<[^>]*(>|$)", string.Empty));
            if (UpdatedLinkedInUrl != null)
                UpdatedLinkedInUrl = WebUtility.HtmlDecode(Regex.Replace(UpdatedLinkedInUrl, "<[^>]*(>|$)", string.Empty));
            if (UpdatedStackOverflowUrl != null)
                UpdatedStackOverflowUrl = WebUtility.HtmlDecode(Regex.Replace(UpdatedStackOverflowUrl, "<[^>]*(>|$)", string.Empty));
            if (UpdatedGithubUrl != null)
                UpdatedGithubUrl = WebUtility.HtmlDecode(Regex.Replace(UpdatedGithubUrl, "<[^>]*(>|$)", string.Empty));
            if (UpdatedTwitterUrl != null)
                UpdatedTwitterUrl = WebUtility.HtmlDecode(Regex.Replace(UpdatedTwitterUrl, "<[^>]*(>|$)", string.Empty));
            if (UpdatedFacebookUrl != null)
                UpdatedFacebookUrl = WebUtility.HtmlDecode(Regex.Replace(UpdatedFacebookUrl, "<[^>]*(>|$)", string.Empty));

            StringBuilder validationErrors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(UpdatedFirstName) && string.IsNullOrWhiteSpace(this.subscriber.FirstName))
                validationErrors.Append("First name cannot be empty,");
            if (string.IsNullOrWhiteSpace(UpdatedLastName) && string.IsNullOrWhiteSpace(this.subscriber.LastName))
                validationErrors.Append("Last name cannot be empty,");
            if (!string.IsNullOrWhiteSpace(UpdatedLinkedInUrl) && !Regex.Match(UpdatedLinkedInUrl, @"^http(s)?://([\w]+.)?linkedin.com/in/[A-z0-9_]+/?$").Success)
                validationErrors.Append("The provided LinkedIn URL is not valid,");
            if (!string.IsNullOrWhiteSpace(UpdatedStackOverflowUrl) && !Regex.Match(UpdatedStackOverflowUrl, @"^http(s)?://([\w]+.)?stackoverflow.com/[A-z0-9_]+/?$").Success)
                validationErrors.Append("The provided StackOverflow URL is not valid,");
            if (!string.IsNullOrWhiteSpace(UpdatedGithubUrl) && !Regex.Match(UpdatedGithubUrl, @"^http(s)?://([\w]+.)?github.com/[A-z0-9_]+/?$").Success)
                validationErrors.Append("The provided GitHub URL is not valid,");
            if (!string.IsNullOrWhiteSpace(UpdatedTwitterUrl) && !Regex.Match(UpdatedTwitterUrl, @"^http(s)?://([\w]+.)?twitter.com/[A-z0-9_]+/?$").Success)
                validationErrors.Append("The provided Twitter URL is not valid,");
            if (!string.IsNullOrWhiteSpace(UpdatedFacebookUrl) && !Regex.Match(UpdatedFacebookUrl, @"^http(s)?://([\w]+.)?facebook.com/[A-z0-9_]+/?$").Success)
                validationErrors.Append("The provided Facebook URL is not valid,");
            if (!string.IsNullOrWhiteSpace(UpdatedPhoneNumber) && !Regex.Match(UpdatedPhoneNumber, @"^\d{10}$").Success)
                validationErrors.Append("The provided phone number must be 10 digits with no formatting,");

            if (validationErrors.Length == 0)
            {
                SubscriberDto Subscriber = new SubscriberDto
                {
                    FirstName = UpdatedFirstName,
                    LastName = UpdatedLastName,
                    Address = UpdatedAddress,
                    PhoneNumber = UpdatedPhoneNumber,
                    City = UpdatedCity,
                    // StateId = UpdatedState,
                    SelectedState = SelectedState,
                    FacebookUrl = UpdatedFacebookUrl,
                    TwitterUrl = UpdatedTwitterUrl,
                    LinkedInUrl = UpdatedLinkedInUrl,
                    StackOverflowUrl = UpdatedStackOverflowUrl,
                    GithubUrl = UpdatedGithubUrl,
                    SubscriberGuid = CurrentSubscriberGuid
                };
                API.UpdateProfileInformation(Subscriber);
                return new BasicResponseDto
                {
                    StatusCode = "200",
                    Description = "OK"
                };
            }
            else
            {
                return new BasicResponseDto
                {
                    StatusCode = "400",
                    Description = validationErrors.ToString()
                };
            }
        }

        [HttpPost]
        public IActionResult ContactUs(string ContactUsFirstName,
            string ContactUsLastName,
            string ContactUsEmail,
            string ContactUsType,
            string ContactUsComment)
        {
            var client = new SendGridClient(_configuration["Sendgrid:ApiKey"]);
            var from = new EmailAddress(_configuration["Sendgrid:EmailSender"]);
            var subject = _configuration["Sendgrid:EmailSubject"];
            var to = new EmailAddress(_configuration["Sendgrid:EmailRecipient"]);

            string firstName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(ContactUsFirstName) ? "No first name enetered." : ContactUsFirstName);
            string lastName = HttpUtility.HtmlEncode(string.IsNullOrEmpty(ContactUsLastName) ? "No last name entered." : ContactUsLastName);
            string email = HttpUtility.HtmlEncode(string.IsNullOrEmpty(ContactUsEmail) ? "No email entered." : ContactUsEmail);
            string type = HttpUtility.HtmlEncode(string.IsNullOrEmpty(ContactUsType) ? "No type entered." : ContactUsType);
            string comment = HttpUtility.HtmlEncode(string.IsNullOrEmpty(ContactUsComment) ? "No comment entered." : ContactUsComment);

            var emailBody = FormatContactEmail(firstName, lastName, email, type, comment);
            var plainTextContent = emailBody;
            var htmlContent = emailBody;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg);
            return RedirectToAction("About", new AboutViewModel());
        }

        private string FormatContactEmail(string ContactUsFirstName,
            string ContactUsLastName,
            string ContactUsEmail,
            string ContactUsType,
            string ContactUsComment)
        {
            StringBuilder emailBody = new StringBuilder("<strong>New email submitted by: </strong>" + ContactUsLastName + ", " + ContactUsFirstName);
            emailBody.Append("<p><strong>User's email: </strong>" + ContactUsEmail + "</p>");
            emailBody.Append("<p><strong>Contact topic: </strong>" + ContactUsType + "</p>");
            emailBody.Append("<p><strong>Message: </strong></p>");
            emailBody.Append("<p>" + ContactUsComment + "</p>");
            return emailBody.ToString();
        }

        [AllowAnonymous]
        public IActionResult Unified()
        {
            /* relative paths to static files cannot be used in this view since the content is served by a third party website.
             * we have to use absolute paths, but we still want a way to prevent outdated static files from being cached. 
             * the code below solves for this by adding the referenced file's last modified timestamp as a hash code to the query 
             * string for all static files referenced.
             */
            ViewData["BlueBg"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\images\blue_background_login.jpg").GetHashCode());
            ViewData["Bootstrap"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\lib\boostrap\css\bootstrap.min.css").GetHashCode());
            ViewData["Bundle"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\css\Bundle.css").GetHashCode());
            ViewData["FontAwesome"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\lib\font-awesome\css\all.min.css").GetHashCode());
            ViewData["Icon20"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\images\Icon-20.png").GetHashCode());
            ViewData["SiteMin"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\css\site.min.css").GetHashCode());
            ViewData["Unified"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\css\Unified.css").GetHashCode());
            ViewData["WhiteLogo"] = Math.Abs(System.IO.File.GetLastWriteTime(_env.WebRootPath + @"\images\cc_logo_white.png").GetHashCode());


            return View();
        }

        public IActionResult ComingSoon()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Api()
        {
            string responseString = "";
            try
            {
                // Retrieve the token with the specified scopes
                var scope = AzureAdB2COptions.ApiScopes.Split(' ');
                string signedInUserID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                TokenCache userTokenCache = new MSALSessionCache(signedInUserID, this.HttpContext).GetMsalCacheInstance();
                ConfidentialClientApplication cca = new ConfidentialClientApplication(AzureAdB2COptions.ClientId, AzureAdB2COptions.Authority, AzureAdB2COptions.RedirectUri, new ClientCredential(AzureAdB2COptions.ClientSecret), userTokenCache, null);
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, cca.Users.FirstOrDefault(), AzureAdB2COptions.Authority, false);

                HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpGetClientName);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, AzureAdB2COptions.ApiUrl);

                // Add token to the Authorization header and make the request
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responseString = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.Unauthorized:
                        responseString = $"Please sign in again. {response.ReasonPhrase}";
                        break;
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            ViewData["Payload"] = $"{responseString}";
            return View();
        }

        [HttpGet]
        [Route("/Home/TierLevel")]
        public string TierLevel()
        {
            return "{\"Tier\": \"1\"}";
        }

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            Response.StatusCode = 500;
            return View();
        }

        public IActionResult PageNotFound()
        {
            ViewBag.InvalidPath = this.HttpContext.Request.Path;
            Response.StatusCode = 404;
            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return LocalRedirect(returnUrl);
        }
    }
}
