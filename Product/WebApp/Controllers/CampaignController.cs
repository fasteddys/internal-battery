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
using UpDiddyLib.Dto.User;

namespace UpDiddy.Controllers
{
    public class CampaignController : BaseController
    {
        private readonly IHostingEnvironment _env;

        private readonly ButterCMSClient _butterClient;

        public CampaignController(IApi api,
            IConfiguration configuration,
            IHostingEnvironment env)
            : base(api, configuration)
        {
            _env = env;
            _butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
        }
        /*
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
            
            int? gatedFileDownloadMaxAttemptsAllowed = null;
            int tmp;
            if(int.TryParse(LandingPage.Data.Fields.gated_file_download_max_attempts_allowed, out tmp))
            {
                gatedFileDownloadMaxAttemptsAllowed = tmp;
            }

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
                PartnerGuid = LandingPage.Data.Fields.partner.PartnerGuid,
                IsGatedDownload = LandingPage.Data.Fields.isgateddownload,
                GatedDownloadFileUrl = LandingPage.Data.Fields.gated_file_download_file,
                GatedFileDownloadMaxAttemptsAllowed = gatedFileDownloadMaxAttemptsAllowed
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


    */
    }
}
