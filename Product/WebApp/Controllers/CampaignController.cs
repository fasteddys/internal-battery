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

namespace UpDiddy.Controllers
{
    public class CampaignController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public CampaignController(IApi api,
            IConfiguration configuration,
            IHostingEnvironment env)
            : base(api)
        {
            _env = env;
            _configuration = configuration;
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

            // If all checks pass, assemble SignUpDto from information user entered. Included in the SignUpDto is the 
            // campaign phase name that was provided via querystring parameter to the landing page            
            SignUpDto sudto = new SignUpDto
            {
                email = signUpViewModel.Email,
                password = signUpViewModel.Password,
                campaignGuid = signUpViewModel.CampaignGuid,
                campaignPhase = WebUtility.UrlDecode(signUpViewModel.CampaignPhase),
                partnerGuid = signUpViewModel.PartnerGuid
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
        [Route("/[controller]/express-sign-up")]
        public async Task<IActionResult> ExpressSignUpAsync(SignUpViewModel signUpViewModel)
        {
            bool modelHasAllFields = !string.IsNullOrEmpty(signUpViewModel.Email) &&
                !string.IsNullOrEmpty(signUpViewModel.Password) &&
                !string.IsNullOrEmpty(signUpViewModel.ReenterPassword);

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
                return BadRequest(new BasicResponseDto
                {
                    StatusCode = 400,
                    Description = "Unfortunately, an error has occured with your submission. Please try again."
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

            // If all checks pass, assemble SignUpDto from information user entered.
            SignUpDto sudto = new SignUpDto
            {
                email = signUpViewModel.Email,
                password = signUpViewModel.Password,
                campaignGuid = signUpViewModel.CampaignGuid,
                firstName  = signUpViewModel.IsWaitList ? signUpViewModel.FirstName : null,
                lastName  = signUpViewModel.IsWaitList ? signUpViewModel.LastName : null,
                phoneNumber  = signUpViewModel.IsWaitList ? signUpViewModel.PhoneNumber : null,
                referer = Request.Headers["Referer"].ToString(),
                verifyUrl = _configuration["Environment:BaseUrl"].TrimEnd('/') + "/email/confirm-verification/",

                //check for any referrerCode 
                referralCode = Request.Cookies["referrerCode"]==null ? null : Request.Cookies["referrerCode"].ToString(),
                partnerGuid = signUpViewModel.PartnerGuid
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
        public IActionResult ShowCampaignLandingPage(string LandingPageSlug)
        {
            // Grab all query params from the request and put them into dictionary that's passed
            // to ButterCMS call.
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }

            QueryParams.Add(UpDiddyLib.Helpers.Constants.CMS.LEVELS, _configuration["ButterCMS:CareerCirclePages:Levels"]);

            // Create ButterCMS client and call their API to get JSON response of page data values.
            var butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
            PageResponse<CampaignLandingPageViewModel> LandingPage = butterClient.RetrievePage<CampaignLandingPageViewModel>("*", LandingPageSlug, QueryParams);

            // Return 404 if no page data is returned from Butter.
            if (LandingPage == null)
                return NotFound();

            // Assemble ViewModel from results returned in Butter GET call.
            var CampaignLandingPageViewModel = new CampaignLandingPageViewModel
            {
                hero_title = LandingPage.Data.Fields.hero_title,
                hero_image = LandingPage.Data.Fields.hero_image,
                hero_content = LandingPage.Data.Fields.hero_content,
                hero_sub_image = LandingPage.Data.Fields.hero_sub_image,
                content_band_header = LandingPage.Data.Fields.content_band_header,
                content_band_text = LandingPage.Data.Fields.content_band_text,
                partner = LandingPage.Data.Fields.partner,
                IsWaitList = LandingPage.Data.Fields.IsWaitList,
                IsExpressSignUp = true
            };

            return View("CampaignLandingPage", CampaignLandingPageViewModel);
        }
    }
}
