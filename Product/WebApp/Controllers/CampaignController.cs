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

        [HttpGet("/MomProject")]
        public IActionResult MomProject()
        {
            return View();
        }

        [HttpGet]
        [Route("/Community/{CampaignGuid?}/{ContactGuid?}")]
        public async Task<IActionResult> CampaignAsync(Guid CampaignGuid, Guid ContactGuid)
        {
            if (CampaignGuid == Guid.Empty || ContactGuid == Guid.Empty)
            {
                return View("Community", new CampaignViewModel()
                {
                    IsExpressCampaign = true
                });
            }

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
                    IsExpressCampaign = false
                };
                return View("Community", cvm);
            }

            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode);
            }
        }
    }
}
