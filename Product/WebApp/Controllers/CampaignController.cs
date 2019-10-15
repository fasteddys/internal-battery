using ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Helpers;
using UpDiddy.ViewModels.ButterCMS;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace UpDiddy.Controllers
{
    public class CampaignController : BaseController
    {
        private readonly IHostingEnvironment _env;

        private readonly ButterCMSClient _butterClient;

        public CampaignController(IApi api,
            IConfiguration configuration,
            IHostingEnvironment env)
            : base(api,configuration)
        {
            _env = env;
            _configuration = configuration;
            _butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
            _maxCookieLength = int.Parse(_configuration["CareerCircle:MaxCookieLength"]);
        }

        [HttpGet("/Wozu")]
        [HttpGet("/MomProject")]
        [HttpGet("/ClinicalResearchFastrack")]
        [HttpGet("/barnett")]
        public IActionResult PartnerLandingPages()
        {
            string viewName = Request.Path.Value.TrimStart('/');

            return View(viewName, new SignUpViewModel()
            {
                IsExpressSignUp = true
            });
        }


        [HttpGet]
        [Route("/lp")]
        public IActionResult SeedEmails()
        {
            /* the call to action from seed emails will land here. doing this because of the following statement in user story 827:
             *      "The message in the seed emails can be similar but not exact- we should have the button labeled similarly but 
             *      it doesn't need to invoke a sign up type experience- the wording on the link and buttons is important and possibly 
             *      sending in a similarly formed url to encourage the data science to treat lead emails like the seed list emails 
             *      if that makes sense."
             *      
             * we can make this whatever we want, but for now it will cycle through random images and display some text to remind people
             * about why we are sending these emails.
             */

            var images = _env.WebRootFileProvider.GetDirectoryContents(@"/images/random");
            int index = new Random().Next(0, images.Count());
            ViewBag.ImageName = images.ElementAt(index).Name;
            return View("SeedEmails");
        }



        [HttpGet]
        [Route("/lp/{tinyId}")]
        public async Task<IActionResult> TargetedCampaignLandingPageAsync(string tinyId)
        {
            if (string.IsNullOrWhiteSpace(tinyId))
                return View("Community", new CampaignViewModel()
                {
                    IsExpressCampaign = true,
                    IsActive = true
                });
            try
            {
                CampaignPartnerContactDto campaignPartnerContact = await _Api.GetCampaignPartnerContactAsync(tinyId);

                if (string.IsNullOrWhiteSpace(tinyId) || campaignPartnerContact == null)
                    return View("Community", new CampaignViewModel()
                    {
                        IsExpressCampaign = true,
                        IsActive = true
                    });

                // capture any campaign phase information that has been passed.  
                string campaignPhase = Request.Query[Constants.TRACKING_KEY_CAMPAIGN_PHASE].ToString();

                // build trackign string url
                string _TrackingImgSource = _configuration["Api:ApiUrl"] +
                    "tracking?contact=" +
                    campaignPartnerContact.PartnerContactGuid.ToString() +
                    "&action=47D62280-213F-44F3-8085-A83BB2A5BBE3&campaign=" +
                    campaignPartnerContact.CampaignGuid.ToString() + "&campaignphase=" + WebUtility.UrlEncode(campaignPhase);

                // hide the details of the user's email
                string obfuscatedEmail = Utils.ObfuscateEmail(campaignPartnerContact.Email);

                CampaignViewModel cvm = new CampaignViewModel()
                {

                    CampaignGuid = campaignPartnerContact.CampaignGuid,
                    PartnerContactGuid = campaignPartnerContact.PartnerContactGuid,
                    TrackingImgSource = _TrackingImgSource,
                    CampaignCourse = null, // no support for campaign course association and rebates
                    CampaignPhase = campaignPhase,
                    IsExpressCampaign = false,
                    IsActive = campaignPartnerContact.IsCampaignActive,
                    ObfuscatedEmail = obfuscatedEmail
                };

                string viewName = campaignPartnerContact.TargetedViewName == null ? "Community" : string.Format("TargetedPages/{0}", campaignPartnerContact.TargetedViewName);
                return View(viewName, cvm);
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode);
            }
        }

        [HttpGet]
        [Route("/Community/{CampaignGuid?}/{PartnerContactGuid?}")]
        [Route("/Join/{CampaignGuid?}/{PartnerContactGuid?}")]
        [Route("/JoinNow/{CampaignGuid?}/{PartnerContactGuid?}")]
        [Route("/Home/Campaign/{CampaignViewName}/{CampaignGuid}/{PartnerContactGuid}")]
        public async Task<IActionResult> CampaignLandingPagesAsync(Guid CampaignGuid, Guid PartnerContactGuid, string CampaignViewName)
        {
            if (CampaignGuid == Guid.Empty || PartnerContactGuid == Guid.Empty)
            {
                return View("Community", new CampaignViewModel()
                {
                    IsExpressCampaign = true,
                    IsActive = true
                });
            }

            try
            {
                // Todo - re-factor once courses and campaigns aren't a 1:1 mapping
                CampaignDto campaign = await _Api.GetCampaignAsync(CampaignGuid);
                ContactDto Contact = await _Api.ContactAsync(PartnerContactGuid);
                CourseDto Course = await _Api.GetCourseByCampaignGuidAsync(CampaignGuid);

                // capture any campaign phase information that has been passed.  
                string CampaignPhase = Request.Query[Constants.TRACKING_KEY_CAMPAIGN_PHASE].ToString();

                // cookie campaign tracking information for future tracking enhacments, such as
                // task 304
                Response.Cookies.Append(Constants.TRACKING_KEY_CAMPAIGN_PHASE, CampaignPhase);
                Response.Cookies.Append(Constants.TRACKING_KEY_CAMPAIGN, CampaignGuid.ToString());
                Response.Cookies.Append(Constants.TRACKING_KEY_PARTNER_CONTACT, PartnerContactGuid.ToString());
                // build trackign string url
                string _TrackingImgSource = _configuration["Api:ApiUrl"] +
                    "tracking?contact=" +
                    Contact.PartnerContactGuid.Value.ToString() +
                    "&action=47D62280-213F-44F3-8085-A83BB2A5BBE3&campaign=" +
                    CampaignGuid + "&campaignphase=" + WebUtility.UrlEncode(CampaignPhase);

                string obfuscatedEmail = Utils.ObfuscateEmail(Contact.Email);
                CampaignViewModel cvm = new CampaignViewModel()
                {

                    CampaignGuid = CampaignGuid,
                    PartnerContactGuid = Contact.PartnerContactGuid.Value,
                    TrackingImgSource = _TrackingImgSource,
                    CampaignCourse = Course,
                    CampaignPhase = CampaignPhase,
                    IsExpressCampaign = false,
                    IsActive = (campaign.EndDate == null || campaign.EndDate > DateTime.UtcNow),
                    ObfuscatedEmail = obfuscatedEmail
                };


                string viewName = CampaignViewName == null ? "Community" : string.Format("TargetedPages/{0}", CampaignViewName);
                return View(viewName, cvm);
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode);
            }
        }

        [HttpPost]
        [Route("/[controller]/sign-up")]
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

            PageResponse<CampaignLandingPageViewModel> landingPage = await GetButterLandingPage(signUpViewModel.CampaignSlug);

            // If all checks pass, assemble SignUpDto from information user entered. Included in the SignUpDto is the 
            // campaign phase name that was provided via querystring parameter to the landing page            
            SignUpDto sudto = new SignUpDto
            {
                email = signUpViewModel.Email,
                password = signUpViewModel.Password,
                campaignGuid = signUpViewModel.CampaignGuid,
                campaignPhase = WebUtility.UrlDecode(signUpViewModel.CampaignPhase),
                partnerGuid = landingPage.Data.Fields.partner.PartnerGuid != null ? landingPage.Data.Fields.partner.PartnerGuid : Guid.Empty,
                subscriberSource = Request.Cookies["source"] == null ? null : Utils.AlphaNumeric(Request.Cookies["source"].ToString(),_maxCookieLength)
            };

            // Guard UX from any unforeseen server error.
            try
            {
                // Convert contact to subscriber and create ADB2C account for them.
                BasicResponseDto subscriberResponse = await _Api.UpdateSubscriberContactAsync(signUpViewModel.PartnerContactGuid, sudto);

                CourseDto Course = await _Api.GetCourseByCampaignGuidAsync((Guid)signUpViewModel.CampaignGuid);
                if (Course == null)
                {
                    return new BasicResponseDto
                    {
                        StatusCode = subscriberResponse.StatusCode,
                        Description = "/Home/Signup"
                    };
                }

                return new BasicResponseDto
                {
                    StatusCode = subscriberResponse.StatusCode,
                    Description = "/Course/Checkout/" + Course.Slug
                };
            }
            catch (ApiException ex)
            {
                Response.StatusCode = (int)ex.StatusCode;
                return ex.ResponseDto;
            }
        }

        [HttpPost]
        [Route("/[controller]/existing-user-sign-up")]
        public async Task<IActionResult> SignupExistingUser(SignUpViewModel signUpViewModel)
        {
            try
            {
                BasicResponseDto subscriberResponse;
                PageResponse<CampaignLandingPageViewModel> landingPage = await GetButterLandingPage(signUpViewModel.CampaignSlug);
                if (landingPage.Data.Fields.iswaitlist)
                {
                    bool modelHasAllFields = !string.IsNullOrEmpty(signUpViewModel.FirstName) && !string.IsNullOrEmpty(signUpViewModel.LastName);
                    if (!modelHasAllFields)
                    {
                        return BadRequest(new BasicResponseDto
                        {
                            StatusCode = 400,
                            Description = "Please enter all sign-up fields and try again."
                        });
                    }

                    var phoneModelState = ModelState.Where(x => x.Key == "PhoneNumber").FirstOrDefault().Value;
                    if (phoneModelState.Errors.Count > 0)
                    {
                        return BadRequest(new BasicResponseDto
                        {
                            StatusCode = 400,
                            Description = phoneModelState.Errors.FirstOrDefault().ErrorMessage
                        });
                    }
                }

                SignUpDto sudto = new SignUpDto
                {
                    email = signUpViewModel.Email,
                    password = signUpViewModel.Password,
                    campaignGuid = signUpViewModel.CampaignGuid,
                    firstName = landingPage.Data.Fields.iswaitlist ? signUpViewModel.FirstName : null,
                    lastName = landingPage.Data.Fields.iswaitlist ? signUpViewModel.LastName : null,
                    phoneNumber = landingPage.Data.Fields.iswaitlist ? signUpViewModel.PhoneNumber : null,
                    subscriberGuid = signUpViewModel.SubscriberGuid,
                    referer = Request.Headers["Referer"].ToString(),
                    verifyUrl = _configuration["Environment:BaseUrl"].TrimEnd('/') + "/email/confirm-verification/",
                    isGatedDownload = landingPage.Data.Fields.isgateddownload && !string.IsNullOrEmpty(landingPage.Data.Fields.gated_file_download_file),
                    gatedDownloadFileUrl = landingPage.Data.Fields.gated_file_download_file,
                    gatedDownloadMaxAttemptsAllowed = !string.IsNullOrEmpty(landingPage.Data.Fields.gated_file_download_max_attempts_allowed) ? (int)Double.Parse(landingPage.Data.Fields.gated_file_download_max_attempts_allowed) : (int?)null,
                    referralCode = Request.Cookies["referrerCode"] == null ? null : Request.Cookies["referrerCode"].ToString(),
                    partnerGuid = landingPage.Data.Fields.partner.PartnerGuid != null ? landingPage.Data.Fields.partner.PartnerGuid : Guid.Empty
                };

                subscriberResponse = await _Api.ExistingUserGroupSignup(sudto);
                switch (subscriberResponse.StatusCode)
                {
                    case 200:
                        return Ok(new BasicResponseDto
                        {
                            StatusCode = 200,
                            Description = "/"
                        });
                    default:
                        return StatusCode(500, subscriberResponse);
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

        [HttpPost]
        [Route("/[controller]/express-sign-up")]
        public async Task<IActionResult> ExpressSignUpAsync(SignUpViewModel signUpViewModel)
        {
            bool modelHasAllFields = !string.IsNullOrEmpty(signUpViewModel.Email) &&
                !string.IsNullOrEmpty(signUpViewModel.Password) &&
                !string.IsNullOrEmpty(signUpViewModel.ReenterPassword);

            //Check the fields if it is a waitlist
            if (signUpViewModel.IsWaitList)
            {
                modelHasAllFields = !string.IsNullOrEmpty(signUpViewModel.FirstName) &&
                !string.IsNullOrEmpty(signUpViewModel.LastName);
            }

            // Make sure user has filled out all fields.
            if (!modelHasAllFields)
            {
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Please enter all sign-up fields and try again."
                });
            }

            // This is basically the same check as above, but to be safe...
            if (!ModelState.IsValid)
            {
                string desc = string.Join("; ", ModelState.Values
                                      .SelectMany(x => x.Errors)
                                      .Select(x => x.ErrorMessage));
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = desc
                });
            }

            // Make sure user's password and re-enter password values match.
            if (!signUpViewModel.Password.Equals(signUpViewModel.ReenterPassword))
            {
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 403,
                    Description = "User's passwords do not match."
                });
            }

            PageResponse<CampaignLandingPageViewModel> landingPage = await GetButterLandingPage(signUpViewModel.CampaignSlug);
            SignUpDto sudto = new SignUpDto
            {
                email = signUpViewModel.Email,
                password = signUpViewModel.Password,
                campaignGuid = signUpViewModel.CampaignGuid,
                firstName = landingPage.Data.Fields.iswaitlist ? signUpViewModel.FirstName : null,
                lastName = landingPage.Data.Fields.iswaitlist ? signUpViewModel.LastName : null,
                phoneNumber = landingPage.Data.Fields.iswaitlist ? signUpViewModel.PhoneNumber : null,
                referer = Request.Headers["Referer"].ToString(),
                verifyUrl = _configuration["Environment:BaseUrl"].TrimEnd('/') + "/email/confirm-verification/",
                isGatedDownload = landingPage.Data.Fields.isgateddownload && !string.IsNullOrEmpty(landingPage.Data.Fields.gated_file_download_file),
                gatedDownloadFileUrl = landingPage.Data.Fields.gated_file_download_file,
                gatedDownloadMaxAttemptsAllowed = !string.IsNullOrEmpty(landingPage.Data.Fields.gated_file_download_max_attempts_allowed) ? (int)Double.Parse(landingPage.Data.Fields.gated_file_download_max_attempts_allowed) : (int?)null,
                partnerGuid = landingPage.Data.Fields.partner.PartnerGuid != null ? landingPage.Data.Fields.partner.PartnerGuid : Guid.Empty,

                //check for any referrerCode 
                referralCode = Request.Cookies["referrerCode"]==null ? null : Utils.AlphaNumeric(Request.Cookies["referrerCode"].ToString(), _maxCookieLength),
                subscriberSource = Request.Cookies["source"] == null ? null : Utils.AlphaNumeric(Request.Cookies["source"].ToString(),_maxCookieLength),
            };

            // Guard UX from any unforeseen server error.
            try
            {
                // Convert contact to subscriber and create ADB2C account for them.
                BasicResponseDto subscriberResponse = await _Api.ExpressUpdateSubscriberContactAsync(sudto);

                switch (subscriberResponse.StatusCode)
                {

                    case 200:
                        // Return url to course checkout page to front-end. This will prompt user to log in
                        // now that their ADB2C account is created.
                        return Ok(new BasicResponseDto
                        {
                            StatusCode = subscriberResponse.StatusCode,
                            Description = "/session/signin"
                        });
                    default:
                        // If there's an error from contact-to-subscriber converstion API call,
                        // return that error description to a toast to the user.
                        return StatusCode(500, subscriberResponse);
                }
            }
            catch (ApiException e)
            {
                // Generic server error to display gracefully to the user.
                return StatusCode(500, new BasicResponseDto
                {
                    StatusCode = 500,
                    Description = e.ResponseDto.Description
                });
            }


        }

        [Route("/campaign/{LandingPageSlug}")]
        public async Task<IActionResult> ShowCampaignLandingPage(string LandingPageSlug)
        {
            PageResponse<CampaignLandingPageViewModel> LandingPage = await GetButterLandingPage(LandingPageSlug);

            SubscriberDto subscriber = null;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                subscriber = await _Api.GetSubscriberByGuid(loggedInUserGuid);
            }

            // Return 404 if no page data is returned from Butter.
            if (LandingPage == null)
                return NotFound();

            // Assemble ViewModel from results returned in Butter GET call.
            SignUpViewModel signUpViewModel = new SignUpViewModel()
            {
                CampaignSlug = LandingPageSlug,
                IsExpressSignUp = true,
                Email = subscriber != null ? subscriber.Email : null,
                FirstName = subscriber != null ? subscriber.FirstName : null,
                LastName = subscriber != null ? subscriber.LastName : null,
                PhoneNumber = subscriber != null ? subscriber.PhoneNumber : null,
                SubscriberGuid = subscriber != null ? subscriber.SubscriberGuid : null,
                SignUpButtonText = LandingPage.Data.Fields.signup_form_submit_button_text,
                SuccessHeader = LandingPage.Data.Fields.success_header,
                SuccessText = LandingPage.Data.Fields.success_text,
                ExistingUserButtonText = LandingPage.Data.Fields.existing_user_button_text,
                ExistingUserSubmitButtonText = LandingPage.Data.Fields.existing_user_form_submit_button_text,
                ExistingUserSuccessHeader = LandingPage.Data.Fields.existing_user_success_header,
                ExistingUserSuccessText = LandingPage.Data.Fields.existing_user_success_text,
                IsWaitList = LandingPage.Data.Fields.iswaitlist,
            };

            var CampaignLandingPageViewModel = new CampaignLandingPageViewModel
            {
                hero_title = LandingPage.Data.Fields.hero_title,
                hero_image = LandingPage.Data.Fields.hero_image,
                hero_content = LandingPage.Data.Fields.hero_content,
                hero_sub_image = LandingPage.Data.Fields.hero_sub_image,
                content_band_header = LandingPage.Data.Fields.content_band_header,
                content_band_text = LandingPage.Data.Fields.content_band_text,
                signup_form_header = LandingPage.Data.Fields.signup_form_header,
                signup_form_text = LandingPage.Data.Fields.signup_form_text,
                existing_user_form_header = LandingPage.Data.Fields.existing_user_form_header,
                existing_user_form_text = LandingPage.Data.Fields.existing_user_form_text,
                isLoggedIn = HttpContext.User.Identity.IsAuthenticated,
                iswaitlist = LandingPage.Data.Fields.iswaitlist,
                signUpViewModel = signUpViewModel
            };
            return View("CampaignLandingPage", CampaignLandingPageViewModel);
        }

        [Route("/salesforcesignup")]
        public IActionResult RedirecttoSalesForceWaitList()
        {
            var redirectUrl = "/campaign/salesforce-waitlist";
            return Redirect(redirectUrl);
        }

        private async Task<PageResponse<CampaignLandingPageViewModel>> GetButterLandingPage(string landingPageSlug)
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            QueryParams.Add(UpDiddyLib.Helpers.Constants.CMS.LEVELS, _configuration["ButterCMS:CareerCirclePages:Levels"]);
            var butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
            PageResponse<CampaignLandingPageViewModel> landingPage = await butterClient.RetrievePageAsync<CampaignLandingPageViewModel>("*", landingPageSlug, QueryParams);
            return landingPage;
        }
    }
}
