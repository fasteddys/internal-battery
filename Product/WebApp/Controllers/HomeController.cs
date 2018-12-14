using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace UpDiddy.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        [HttpGet]
        public IActionResult GetCountries()
        {
            return Ok(Json(_Api.GetCountries()));
        }

        public HomeController(IApi api,
            IConfiguration configuration,
            IHostingEnvironment env)
            : base(api)
        {
            _env = env;
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult GetStatesByCountry(Guid countryGuid)
        {
            return Ok(Json(_Api.GetStatesByCountry(countryGuid)));
        }

        public IActionResult Index()
        {
            // TODO remove test code 
            GetSubscriber(false);

            HomeViewModel HomeViewModel = new HomeViewModel(_configuration, _Api.Topics());
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

        public IActionResult LoggingIn()
        {
            return View();
        }

        [Authorize]
        public IActionResult ProfileLogin()
        {
            GetSubscriber(true);
            // UPdated the subscribers course progress 
            if (this.subscriber != null)
                _Api.UpdateStudentCourseProgress((Guid)this.subscriber.SubscriberGuid, true);

            return RedirectToAction("Profile", "Home");
        }

        [Authorize]
        public IActionResult Profile()
        {
            GetSubscriber(true);

            ProfileViewModel profileViewModel = new ProfileViewModel()
            {
                SubscriberGuid = this.subscriber?.SubscriberGuid,
                FirstName = this.subscriber?.FirstName,
                LastName = this.subscriber?.LastName,
                FormattedPhone = this.subscriber?.PhoneNumber,
                Email = this.subscriber?.Email,
                Address = this.subscriber?.Address,
                City = this.subscriber?.City,
                SelectedState = this.subscriber?.State?.StateGuid,
                SelectedCountry = this.subscriber?.State?.Country?.CountryGuid,
                FacebookUrl = this.subscriber?.FacebookUrl,
                GithubUrl = this.subscriber?.GithubUrl,
                ImageUrl = null,
                LinkedInUrl = this.subscriber?.LinkedInUrl,
                StackOverflowUrl = this.subscriber?.StackOverflowUrl,
                TwitterUrl = this.subscriber?.TwitterUrl,
                Enrollments = this.subscriber?.Enrollments,
                Countries = _Api.GetCountries().Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString(),
                }),
                States = _Api.GetStatesByCountry(this.subscriber?.State?.Country?.CountryGuid).Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.StateGuid.ToString(),
                    Selected = s.StateGuid == this.subscriber?.State?.StateGuid
                })
            };

            // we have to call this other api method directly because it can trigger a refresh of course progress from Woz.
            // i considered overloading the existing GetSubscriber method to do this, but then that makes CourseController 
            // a dependency of BaseController. that's more refactoring than i think we want to concern ourselves with now.
            foreach (var enrollment in profileViewModel.Enrollments)
            {
                var courseLogin = _Api.CourseLogin(profileViewModel.SubscriberGuid.Value, enrollment.EnrollmentGuid.Value);
                enrollment.CourseUrl = courseLogin.LoginUrl;
            }

            return View(profileViewModel);
        }

        [Authorize]
        [HttpPost]
        public BasicResponseDto UpdateProfileInformation(ProfileViewModel profileViewModel)
        {

            if (ModelState.IsValid)
            {
                SubscriberDto Subscriber = new SubscriberDto
                {
                    FirstName = profileViewModel.FirstName,
                    LastName = profileViewModel.LastName,
                    Address = profileViewModel.Address,
                    PhoneNumber = profileViewModel.Phone,
                    City = profileViewModel.City,
                    State = new StateDto() { StateGuid = profileViewModel.SelectedState },
                    FacebookUrl = profileViewModel.FacebookUrl,
                    TwitterUrl = profileViewModel.TwitterUrl,
                    LinkedInUrl = profileViewModel.LinkedInUrl,
                    StackOverflowUrl = profileViewModel.StackOverflowUrl,
                    GithubUrl = profileViewModel.GithubUrl,
                    SubscriberGuid = profileViewModel.SubscriberGuid
                };
                _Api.UpdateProfileInformation(Subscriber);
                return new BasicResponseDto
                {
                    StatusCode = "200",
                    Description = "OK"
                };
            }
            else
            {
                StringBuilder validationErrors = new StringBuilder();

                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        validationErrors.Append(error.ErrorMessage);
                        validationErrors.Append("|");
                    }
                }

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

        [Authorize]
        [HttpGet]
        public JsonResult GetSkills(string userQuery)
        {
            var matchedSkills = _Api.GetSkills(userQuery);
            return new JsonResult(matchedSkills);
        }
    }
}
