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

        public HomeController(IApi api, IConfiguration configuration, IHostingEnvironment env)
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
            // new

            // don't think we want to change how the subscriber is loaded
            GetSubscriber(true);

            // hydrate the view model object from the dto
            ProfileViewModel profileViewModel = new ProfileViewModel()
            {
                SubscriberGuid = this.subscriber.SubscriberGuid,
                FirstName = this.subscriber.FirstName,
                LastName = this.subscriber.LastName,
                Phone = this.subscriber.PhoneNumber,
                Address = this.subscriber.Address,
                City = this.subscriber.City,
                State = this.subscriber.State,
                Country = null,
                FacebookUrl = this.subscriber.FacebookUrl,
                GithubUrl = this.subscriber.GithubUrl,
                ImageUrl = null,
                LinkedInUrl = this.subscriber.LinkedInUrl,
                StackOverflowUrl = this.subscriber.StackOverflowUrl,
                TwitterUrl = this.subscriber.TwitterUrl,
                Enrollments = this.subscriber.Enrollments,
                // todo: figure out how to retrieve country and state lists and selected values. need to cleanup the api references which are currently using int instead of guid
                Countries = _Api.GetCountries().Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString()
                }),
                States = new List<StateViewModel>().Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.StateGuid.ToString()
                })
            };

            // we have to call this other api method directly because it can trigger a refresh of course progress from Woz.
            // i considered overloading the existing GetSubscriber method to do this, but then that makes CourseController 
            // a dependency of BaseController. that's more refactoring than i think we want to concern ourselves with now.
            foreach(var enrollment in profileViewModel.Enrollments)
            {
                var courseLogin = _Api.CourseLogin(profileViewModel.SubscriberGuid.Value, enrollment.EnrollmentGuid.Value);
                enrollment.CourseUrl = courseLogin.LoginUrl;
            }

            // return view model 
            return View(profileViewModel);

            /* old
            GetSubscriber(true);
            IList<EnrollmentDto> CurrentEnrollments = _Api.GetCurrentEnrollmentsForSubscriber(this.subscriber);
            CountryDto SubscriberCountry = new CountryDto();
            StateDto SubscriberState = new StateDto();
            if (this.subscriber.StateId != 0)
            {
                // JAB: TODO explore "Query Types" as a means to get all of the subscriber information from a view
                //  https://docs.microsoft.com/en-us/ef/core/modeling/query-types                
                SubscriberCountry = _Api.GetSubscriberCountry(this.subscriber.StateId);
                SubscriberState = _Api.GetSubscriberState(this.subscriber.StateId);
            }
            IList<WozCourseProgressDto> WozCourseProgressions = new List<WozCourseProgressDto>();

            if (CurrentEnrollments != null)
            {

                foreach (EnrollmentDto enrollment in CurrentEnrollments)
                {
                    try
                    {
                        Guid CourseGuid = enrollment.Course.CourseGuid ?? default(Guid);
                        Guid VendorGuid = enrollment.Course.Vendor.VendorGuid ?? default(Guid);
                        Guid SubscriberGuid = this.subscriber.SubscriberGuid ?? default(Guid);
                        var courseLogin = _Api.CourseLogin(SubscriberGuid, CourseGuid, VendorGuid);

                        WozCourseProgressDto dto = new WozCourseProgressDto
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

            ProfileViewModel.Countries = _Api.GetCountries().Select(c => new SelectListItem()
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
            */
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
            return new BasicResponseDto();

            /* OLD
             * todo: the validation and sanitization logic below is probably the worst code i have written in the last 10 years. 
             * for the love of god, refactor this to use the following pattern: use IActionResult instead of BasicResponseDto for
             * return type, use data annotations for validation, simplify view model and pass that to method as a single parameter 
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
                    State = SelectedState,
                    FacebookUrl = UpdatedFacebookUrl,
                    TwitterUrl = UpdatedTwitterUrl,
                    LinkedInUrl = UpdatedLinkedInUrl,
                    StackOverflowUrl = UpdatedStackOverflowUrl,
                    GithubUrl = UpdatedGithubUrl,
                    SubscriberGuid = CurrentSubscriberGuid
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
                return new BasicResponseDto
                {
                    StatusCode = "400",
                    Description = validationErrors.ToString()
                };
            }
            */
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
    }
}
