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

namespace UpDiddy.Controllers
{
    public class HomeController : BaseController
    {
        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IConfiguration _configuration;
   

        public HomeController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<HomeController> localizer, IConfiguration configuration) 
            : base(azureAdB2COptions.Value, configuration)
        {


            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
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

        public IActionResult Profile()
        {
            GetSubscriber(true);
            IList<CountryStateDto> CountryStateList = API.GetCountryStateList();
            IList<EnrollmentDto> CurrentEnrollments = API.GetCurrentEnrollmentsForSubscriber(this.subscriber);
            CountryDto SubscriberCountry = new CountryDto();
            StateDto SubscriberState = new StateDto();
            if (this.subscriber.StateId != 0)
            {
                SubscriberCountry = API.GetSubscriberCountry(this.subscriber.StateId);
                SubscriberState = API.GetSubscriberState(this.subscriber.StateId);
            }
            IList<WozCourseProgress> WozCourseProgressions = new List<WozCourseProgress>();
            foreach(EnrollmentDto enrollment in CurrentEnrollments)
            {
                var studentLogin = API.StudentLogin(this.subscriber.SubscriberId);
                
                try
                {
                    WozCourseProgress dto = new WozCourseProgress {
                        CourseName = enrollment.Course.Name,
                        CourseUrl = studentLogin == null ? string.Empty : studentLogin.RegistrationUrl,
                        PercentComplete = enrollment.PercentComplete
                    };
                    
                    WozCourseProgressions.Add(dto);
                }
                catch(Exception e)
                {
                    // Wire up logging for controller exceptions.
                }
            }
            ProfileViewModel ProfileViewModel = new ProfileViewModel(
                _configuration, 
                this.subscriber, 
                CountryStateList, 
                WozCourseProgressions, 
                SubscriberCountry,
                SubscriberState);
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
            Guid CurrentSubscriberGuid
            )
        {
            SubscriberDto Subscriber = new SubscriberDto
            {
                FirstName = UpdatedFirstName,
                LastName = UpdatedLastName,
                Address = UpdatedAddress,
                PhoneNumber = UpdatedPhoneNumber,
                City = UpdatedCity,
                StateId = UpdatedState,
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
            var emailBody = FormatContactEmail(ContactUsFirstName, ContactUsLastName, ContactUsEmail, ContactUsType, ContactUsComment);
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
                // TODO remove debug var
                var x = cca.Users.FirstOrDefault();
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, cca.Users.FirstOrDefault(), AzureAdB2COptions.Authority, false);

                HttpClient client = new HttpClient();
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
