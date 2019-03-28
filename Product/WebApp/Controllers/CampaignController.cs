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
        [HttpGet("/itprotv")]
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
        [Route("/Community/{CampaignGuid?}/{ContactGuid?}")]
        [Route("/Join/{CampaignGuid?}/{ContactGuid?}")]
        [Route("/JoinNow/{CampaignGuid?}/{ContactGuid?}")]
        [Route("/Home/Campaign/{CampaignViewName}/{CampaignGuid}/{ContactGuid}")]
        public async Task<IActionResult> CampaignLandingPagesAsync(Guid CampaignGuid, Guid ContactGuid, string CampaignViewName)
        {
            if (CampaignGuid == Guid.Empty || ContactGuid == Guid.Empty)
            {
                return View("Community", new CampaignViewModel()
                {
                    IsExpressCampaign = true,
                    IsActive = true
                });
            }

            CampaignDto campaign = await _Api.GetCampaignAsync(CampaignGuid);

            // capture any campaign phase information that has been passed.  
            string CampaignPhase = Request.Query[Constants.TRACKING_KEY_CAMPAIGN_PHASE].ToString();

            // cookie campaign tracking information for future tracking enhacments, such as
            // task 304
            Response.Cookies.Append(Constants.TRACKING_KEY_CAMPAIGN_PHASE, CampaignPhase);
            Response.Cookies.Append(Constants.TRACKING_KEY_CAMPAIGN, CampaignGuid.ToString());
            Response.Cookies.Append(Constants.TRACKING_KEY_CONTACT, ContactGuid.ToString());
            // build trackign string url
            string _TrackingImgSource = _configuration["Api:ApiUrl"] +
                "tracking?contact=" +
                ContactGuid +
                "&action=47D62280-213F-44F3-8085-A83BB2A5BBE3&campaign=" +
                CampaignGuid + "&campaignphase=" + WebUtility.UrlEncode(CampaignPhase);

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
                    CampaignCourse = Course,
                    CampaignPhase = CampaignPhase,
                    IsExpressCampaign = false,
                    IsActive = (campaign.EndDate == null || campaign.EndDate > DateTime.UtcNow)
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
                campaignPhase = WebUtility.UrlDecode(signUpViewModel.CampaignPhase)
            };

            // Guard UX from any unforeseen server error.
            try
            {
                // Convert contact to subscriber and create ADB2C account for them.
                BasicResponseDto subscriberResponse = await _Api.UpdateSubscriberContactAsync(signUpViewModel.ContactGuid, sudto);

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
                referer = Request.Headers["Referer"].ToString()
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
    }
}
