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
using System.IO;
using UpDiddyLib.Helpers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Claims;
using UpDiddy.Helpers;
using System.Security.Claims;
using UpDiddyLib.Dto.Marketing;
using UpDiddy.Authentication;
using Microsoft.AspNetCore.Diagnostics;

namespace UpDiddy.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            return Ok(Json(await _Api.GetCountriesAsync()));
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
        public async Task<IActionResult> GetStatesByCountry(Guid countryGuid)
        {
            return Ok(Json(await _Api.GetStatesByCountryAsync(countryGuid)));
        }

        public async Task<IActionResult> Index()
        {
            HomeViewModel HomeViewModel = new HomeViewModel(_configuration, await _Api.TopicsAsync());
            return View(HomeViewModel);
        }

        public IActionResult TermsOfService()
        {
            return View();
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        public async Task<IActionResult> SignUp()
        {
            // This will check to see if the subscriber has onboarded. If not, it flips the flag.
            // This means the onboarding flow should only ever work the first time a user logs into their account.
            if (subscriber.HasOnboarded != 1)
                await _Api.UpdateOnboardingStatusAsync();

            var countries = await _Api.GetCountriesAsync();
            var states = await _Api.GetStatesByCountryAsync(this.subscriber?.State?.Country?.CountryGuid);
            SignupFlowViewModel signupFlowViewModel = new SignupFlowViewModel()
            {
                SubscriberGuid = (Guid)subscriber.SubscriberGuid,
                Countries = countries.Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString(),
                }),
                States = states.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.StateGuid.ToString(),
                    Selected = s.StateGuid == this.subscriber?.State?.StateGuid
                }),
                Skills = new List<SkillDto>()// _Api.GetSkillsBySubscriber(this.subscriber.SubscriberGuid.Value)
            };
            return View(signupFlowViewModel);
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

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        public async Task<IActionResult> ProfileLogin()
        {
            // todo: consider updating the course status on the API side when a request is made to retrieve the courses or something instead of
            // logic being determined in web app for managing API data

            await _Api.UpdateStudentCourseProgressAsync(true);

            if (this.subscriber.HasOnboarded > 0)
                return RedirectToAction("Profile", "Home");
            else
                return RedirectToAction("Signup", "Home");
        }

        [LoadSubscriber(isHardRefresh: true, isSubscriberRequired: true)]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var countries = await _Api.GetCountriesAsync();
            var states = await _Api.GetStatesByCountryAsync(this.subscriber?.State?.Country?.CountryGuid);

            ProfileViewModel profileViewModel = new ProfileViewModel()
            {
                SubscriberGuid = this.subscriber?.SubscriberGuid,
                FirstName = this.subscriber?.FirstName,
                LastName = this.subscriber?.LastName,
                FormattedPhone = this.subscriber?.PhoneNumber,
                Email = this.subscriber?.Email,
                Address = UpDiddyLib.Helpers.Utils.ToTitleCase(this.subscriber?.Address),
                City = UpDiddyLib.Helpers.Utils.ToTitleCase(this.subscriber?.City),
                PostalCode = this.subscriber?.PostalCode,
                SelectedState = this.subscriber?.State?.StateGuid,
                SelectedCountry = this.subscriber?.State?.Country?.CountryGuid,
                FacebookUrl = this.subscriber?.FacebookUrl,
                GithubUrl = this.subscriber?.GithubUrl,
                ImageUrl = null,
                LinkedInUrl = this.subscriber?.LinkedInUrl,
                StackOverflowUrl = this.subscriber?.StackOverflowUrl,
                TwitterUrl = this.subscriber?.TwitterUrl,
                Enrollments = this.subscriber?.Enrollments,
                WorkCompensationTypes = await _Api.GetCompensationTypesAsync(),
                EducationDegreeTypes = await _Api.GetEducationalDegreeTypesAsync(),
                Countries = countries.Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString(),
                }),
                States = states.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.StateGuid.ToString(),
                    Selected = s.StateGuid.Equals(this.subscriber?.State?.StateGuid)
                }),
                // todo: consider refactoring this... include in GetSubscriber (add navigation property)
                Skills = await _Api.GetSkillsBySubscriberAsync(this.subscriber.SubscriberGuid.Value),
                Files = this.subscriber?.Files,
                WorkHistory = await _Api.GetWorkHistoryAsync(this.subscriber.SubscriberGuid.Value),
                EducationHistory = await _Api.GetEducationHistoryAsync(this.subscriber.SubscriberGuid.Value)
            };

            // we have to call this other api method directly because it can trigger a refresh of course progress from Woz.
            // i considered overloading the existing GetSubscriber method to do this, but then that makes CourseController 
            // a dependency of BaseController. that's more refactoring than i think we want to concern ourselves with now.
            foreach (var enrollment in profileViewModel.Enrollments)
            {
                var courseLogin = await _Api.CourseLoginAsync(enrollment.EnrollmentGuid.Value);
                enrollment.CourseUrl = courseLogin.LoginUrl;
            }

            return View(profileViewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<BasicResponseDto> UpdateProfileInformation(ProfileViewModel profileViewModel)
        {
            List<SkillDto> skillsDto = null;
            if (ModelState.IsValid)
            {
                if (profileViewModel.SelectedSkills != null)
                {
                    var skills = profileViewModel.SelectedSkills.Split(',');
                    if (skills.Length > 0)
                    {
                        skillsDto = new List<SkillDto>();
                        foreach (var skill in skills)
                        {
                            Guid parsedGuid;
                            if (Guid.TryParse(skill, out parsedGuid))
                                skillsDto.Add(new SkillDto() { SkillGuid = parsedGuid });
                        }
                    }
                }

                SubscriberDto Subscriber = new SubscriberDto
                {
                    FirstName = profileViewModel.FirstName,
                    LastName = profileViewModel.LastName,
                    Address = profileViewModel.Address,
                    PhoneNumber = profileViewModel.Phone,
                    PostalCode = profileViewModel.PostalCode,
                    City = profileViewModel.City,
                    State = new StateDto() { StateGuid = profileViewModel.SelectedState },
                    FacebookUrl = profileViewModel.FacebookUrl,
                    TwitterUrl = profileViewModel.TwitterUrl,
                    LinkedInUrl = profileViewModel.LinkedInUrl,
                    StackOverflowUrl = profileViewModel.StackOverflowUrl,
                    GithubUrl = profileViewModel.GithubUrl,
                    SubscriberGuid = profileViewModel.SubscriberGuid,
                    Skills = skillsDto
                };
                await _Api.UpdateProfileInformationAsync(Subscriber);
                return new BasicResponseDto
                {
                    StatusCode = 200,
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
                    StatusCode = 400,
                    Description = validationErrors.ToString()
                };
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadFile(Guid fileGuid)
        {
            HttpResponseMessage response = await _Api.DownloadFileAsync(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), fileGuid);
            Stream stream = await response.Content.ReadAsStreamAsync();
            return File(stream, "application/octet-stream",
                response.Content.Headers.ContentDisposition.FileName.Replace("\"", ""));
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> OnboardAsync(SignupFlowViewModel signupFlowViewModel)
        {
            List<SkillDto> skillsDto = null;
            if (ModelState.IsValid)
            {
                if (signupFlowViewModel.SelectedSkills != null)
                {
                    var skills = signupFlowViewModel.SelectedSkills.Split(',');
                    if (skills.Length > 0)
                    {
                        skillsDto = new List<SkillDto>();
                        foreach (var skill in skills)
                        {
                            Guid parsedGuid;
                            if (Guid.TryParse(skill, out parsedGuid))
                                skillsDto.Add(new SkillDto() { SkillGuid = parsedGuid });
                        }
                    }
                }

                SubscriberDto Subscriber = new SubscriberDto
                {
                    FirstName = signupFlowViewModel.FirstName,
                    LastName = signupFlowViewModel.LastName,
                    Address = signupFlowViewModel.Address,
                    PhoneNumber = signupFlowViewModel.Phone,
                    City = signupFlowViewModel.City,
                    PostalCode = signupFlowViewModel.PostalCode,
                    State = new StateDto() { StateGuid = signupFlowViewModel.SelectedState },
                    SubscriberGuid = (Guid)this.subscriber.SubscriberGuid,
                    Skills = skillsDto
                };
                await _Api.UpdateProfileInformationAsync(Subscriber);
                return RedirectToAction("Profile");
            }
            else
            {
                // todo: implement logic to tell user modelstate was invalid
                return RedirectToAction("Profile");
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

        public IActionResult PageNotFound()
        {
            ViewBag.OriginalPath = this.HttpContext.Request.Path;
            return View("404");
        }

        public IActionResult Error(int? statusCode = null)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            ViewBag.OriginalPath = feature?.OriginalPath;
            if (statusCode.HasValue)
            {
                if (statusCode.Value == 404 || statusCode.Value == 500)
                {
                    var viewName = statusCode.ToString();
                    return View(viewName);
                }
            }
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

        // TODO find a better home for these lookup endpoints - maybe a new lookup or data endpoint?
        [Authorize]
        [HttpGet]
        [Route("/Home/GetSkills")]
        public async Task<JsonResult> GetSkillsAsync(string userQuery)
        {
            var matchedSkills = await _Api.GetSkillsAsync(userQuery);
            return new JsonResult(matchedSkills);
        }

        [Authorize]
        [HttpGet]
        [Route("/Home/GetCompanies")]
        public async Task<JsonResult> GetCompaniesAsync(string userQuery)
        {
            var matchedCompanies = await _Api.GetCompaniesAsync(userQuery);
            return new JsonResult(matchedCompanies);
        }


        [Authorize]
        [HttpGet]
        [Route("/Home/GetEducationalInstitutions")]
        public async Task<JsonResult> GetEducationalInstitutionsAsync(string userQuery)
        {
            var matchedInstitutions = await _Api.GetEducationalInstitutionsAsync(userQuery);
            return new JsonResult(matchedInstitutions);
        }


        [Authorize]
        [HttpGet]
        [Route("/Home/GetEducationalDegrees")]
        public async Task<JsonResult> GetEducationalDegreesAsync(string userQuery)
        {
            var matchedDegrees = await _Api.GetEducationalDegreesAsync(userQuery);
            return new JsonResult(matchedDegrees);
        }


        [Authorize]
        [HttpGet]
        [Route("/Home/GetCompensationTypes")]
        public async Task<JsonResult> GetCompensationTypesAsync(string userQuery)
        {
            var compensationTypes = await _Api.GetCompaniesAsync(userQuery);
            return new JsonResult(compensationTypes);
        }

        [Authorize]
        [HttpPost]
        [Route("/Home/AddWorkHistory")]
        public async Task<IActionResult> AddWorkHistoryAsync([FromBody] SubscriberWorkHistoryDto wh )
        {
            Guid subscriberGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (wh == null)
                return BadRequest("Oops, We're sorry somthing went wrong!");

            try
            {
                return Ok(await _Api.AddWorkHistoryAsync(subscriberGuid, wh));
            }
            catch (ApiException ex)
            {
                Response.StatusCode = (int)ex.StatusCode;
                return new JsonResult(new BasicResponseDto { StatusCode = (int)ex.StatusCode, Description = "Oops, We're sorry somthing went wrong!" });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/Home/UpdateWorkHistory")]
        public async Task<IActionResult> UpdateWorkHistoryAsync([FromBody] SubscriberWorkHistoryDto wh)
        {
            Guid subscriberGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (wh == null)              
                return BadRequest("Oops, We're sorry somthing went wrong!");

            try
            {
                return Ok(await _Api.UpdateWorkHistoryAsync(subscriberGuid, wh));
            }
            catch(ApiException ex)
            {
                Response.StatusCode = (int)ex.StatusCode;
                return new JsonResult(new BasicResponseDto { StatusCode = (int)ex.StatusCode, Description = "Oops, We're sorry somthing went wrong!" });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/Home/DeleteWorkHistory/{WorkHistoryGuid}")]
        public async Task<IActionResult> DeleteWorkHistoryAsync(Guid WorkHistoryGuid)
        {
            Guid subscriberGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                return Ok(await _Api.DeleteWorkHistoryAsync(subscriberGuid, WorkHistoryGuid));
            }
            catch(ApiException ex)
            {
                Response.StatusCode = (int)ex.StatusCode;
                return new JsonResult(new BasicResponseDto { StatusCode = (int)ex.StatusCode, Description = "Oops, We're sorry somthing went wrong!" });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/Home/AddEducationalHistory")]
        public async Task<IActionResult> AddEducationalHistoryAsync([FromBody] SubscriberEducationHistoryDto eh)
        {
            Guid subscriberGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (eh == null)
                return BadRequest("Oops, We're sorry somthing went wrong!");

            try
            {
                return Ok(await _Api.AddEducationalHistoryAsync(subscriberGuid, eh));
            }
            catch(ApiException ex)
            {
                Response.StatusCode = (int)ex.StatusCode;
                return new JsonResult(new BasicResponseDto { StatusCode = (int)ex.StatusCode, Description = "Oops, We're sorry somthing went wrong!" });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/Home/UpdateEducationHistory")]
        public async Task<IActionResult> UpdateEducationHistoryAsync([FromBody] SubscriberEducationHistoryDto eh)
        {
            Guid subscriberGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (eh == null)
                return BadRequest("Oops, We're sorry somthing went wrong!");

            try
            {
                return Ok(await _Api.UpdateEducationHistoryAsync(subscriberGuid, eh));
            }
            catch (ApiException ex)
            {
                Response.StatusCode = (int)ex.StatusCode;
                return new JsonResult(new BasicResponseDto { StatusCode = (int)ex.StatusCode, Description = "Oops, We're sorry somthing went wrong!" });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/Home/DeleteEducationHistory/{EducationHistoryGuid}")]
        public async Task<IActionResult> DeleteEducationHistoryAsync(Guid EducationHistoryGuid)
        {
            Guid subscriberGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            try
            {
                return Ok(await _Api.DeleteEducationHistoryAsync(subscriberGuid, EducationHistoryGuid));
            }
            catch (ApiException ex)
            {
                Response.StatusCode = (int)ex.StatusCode;
                return new JsonResult(new BasicResponseDto { StatusCode = (int)ex.StatusCode, Description = "Oops, We're sorry somthing went wrong!" });
            }
        }

        [HttpGet]
        [Route("/Home/Campaign/{CampaignViewName}/{CampaignGuid}/{ContactGuid}")]
        public async Task<IActionResult> CampaignAsync(string CampaignViewName, Guid CampaignGuid, Guid ContactGuid)
        {
            string _TrackingImgSource = _configuration["Api:ApiUrl"] +
                "tracking?contact=" +
                ContactGuid +
                "&action=47D62280-213F-44F3-8085-A83BB2A5BBE3&campaign=" +
                CampaignGuid;

            try
            {
                // Todo - re-factor once courses and campaigns aren't a 1:1 mapping
                ContactDto Contact = await _Api.ContactAsync(ContactGuid);
                CourseDto Course = await _Api.GetCourseByCampaignGuidAsync(CampaignGuid);

                CampaignViewModel cvm = new CampaignViewModel()
                {
                    CampaignGuid = CampaignGuid,
                    ContactGuid = ContactGuid,
                    TrackingImgSource = _TrackingImgSource,
                    CampaignCourse = Course
                };
                return View("Campaign/" + CampaignViewName, cvm);
            }
            catch (ApiException ex)
            {
                return StatusCode((int) ex.StatusCode);
            }
        }

        [HttpPost]
        [Route("/Home/CampaignSignUp")]
        public async Task<BasicResponseDto> CampaignSignUpAsync(SignUpViewModel signUpViewModel)
        {
            bool modelHasAllFields = !string.IsNullOrEmpty(signUpViewModel.Email) &&
                !string.IsNullOrEmpty(signUpViewModel.Password) &&
                !string.IsNullOrEmpty(signUpViewModel.ReenterPassword);

            // Make sure user has filled out all fields.
            if (!modelHasAllFields)
            {
                Response.StatusCode = 400;
                return new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Please enter all sign-up fields and try again."
                };
            }

            // This is basically the same check as above, but to be safe...
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Unfortunately, an error has occured with your submission. Please try again."
                };
            }

            // Make sure user's password and re-enter password values match.
            if (!signUpViewModel.Password.Equals(signUpViewModel.ReenterPassword))
            {
                Response.StatusCode = 400;
                return new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "User's passwords do not match."
                };
            }

            // If all checks pass, assemble SignUpDto from information user entered.
            SignUpDto sudto = new SignUpDto
            {
                email = signUpViewModel.Email,
                password = signUpViewModel.Password,
                campaignGuid = signUpViewModel.CampaignGuid
            };

            // Guard UX from any unforeseen server error.
            try
            {
                // Convert contact to subscriber and create ADB2C account for them.
                BasicResponseDto subscriberResponse = await _Api.UpdateSubscriberContactAsync(signUpViewModel.ContactGuid, sudto);

                CourseDto Course = await _Api.GetCourseByCampaignGuidAsync((Guid)signUpViewModel.CampaignGuid);
                return new BasicResponseDto
                {
                    StatusCode = subscriberResponse.StatusCode,
                    Description = "/Course/Checkout/" + Course.Slug
                };
            }
            catch(ApiException ex)
            {
                Response.StatusCode = (int) ex.StatusCode;
                return ex.ResponseDto;
            }
        }
    }
}