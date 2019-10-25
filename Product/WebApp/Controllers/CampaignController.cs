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
            _butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
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
