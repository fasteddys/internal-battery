using System.Collections.Generic;
using System.Threading.Tasks;
using ButterCMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.Controllers;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using UpDiddyLib.Helpers;
namespace WebApp.Controllers
{
    public class HireTalentController : BaseController
    {
        private IButterCMSService _butterService;
        public HireTalentController(IApi api,
        IConfiguration configuration,
        IButterCMSService butterService)
         : base(api,configuration)
        {
            _butterService = butterService;
        }

        [HttpGet("hire-talent")]
        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<HireTalentPageViewModel> hireTalentPage = await _butterService.RetrievePageAsync<HireTalentPageViewModel>("/hire-talent", QueryParams);

            if(hireTalentPage == null)
                return NotFound();

            SetSEOTags(hireTalentPage);

            HireTalentPageViewModel model = new HireTalentPageViewModel()
            {
                Header = hireTalentPage.Data.Fields.Header,
                Content = hireTalentPage.Data.Fields.Content,
                Footer = hireTalentPage.Data.Fields.Footer,
                Title = hireTalentPage.Data.Fields.Title,
            };


            return View(model);
        }

        private void SetSEOTags(PageResponse<HireTalentPageViewModel> landingPage)
        {
            ViewData[Constants.Seo.TITLE] = landingPage.Data.Fields.Title;
            ViewData[Constants.Seo.META_DESCRIPTION] = landingPage.Data.Fields.MetaDescription;
            ViewData[Constants.Seo.META_KEYWORDS] = landingPage.Data.Fields.MetaKeywords;
            ViewData[Constants.Seo.OG_TITLE] = landingPage.Data.Fields.OpenGraphTitle;
            ViewData[Constants.Seo.OG_DESCRIPTION] = landingPage.Data.Fields.OpenGraphDescription;
            ViewData[Constants.Seo.OG_IMAGE] = landingPage.Data.Fields.OpenGraphImage;
        }
    }
}