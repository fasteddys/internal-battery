using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddy.Api;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using UpDiddy.ViewModels.Components.Layout;
using UpDiddyLib.Helpers;

namespace UpDiddy.ViewComponents
{
    public class OpenGraphViewComponent : ViewComponent
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        // todo: part of refactor of API updiddy
        private IApi _Api = null;
        private IConfiguration _configuration = null;
        private IDistributedCache _cache = null;
        private IButterCMSService _butterService = null;
        private ISysEmail _sysEmail = null;

        public OpenGraphViewComponent(
            IApi api,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache,
            IButterCMSService butterService,
            ISysEmail sysEmail)
        {
            _Api = api;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _butterService = butterService;
            _sysEmail = sysEmail;
        }

        public IViewComponentResult Invoke(string PagePath)
        {
            
            var ButterResponse = _butterService.RetrievePage<ButterCMSBaseViewModel>($"bcms_page_{PagePath}", PagePath.Split("/").Last());
            ButterCMSBaseViewModel ButterViewModel = new ButterCMSBaseViewModel
            {
                ogtitle = _configuration["SEO:OpenGraph:Title"],
                ogdescription = _configuration["SEO:OpenGraph:Description"],
                ogimage = _configuration["SEO:OpenGraph:Image"]
            };

            if (ButterResponse != null)
            {
                ButterViewModel.ogtitle = ButterResponse.Data.Fields.ogtitle ?? _configuration["SEO:OpenGraph:Title"];
                ButterViewModel.ogdescription = ButterResponse.Data.Fields.ogdescription ?? _configuration["SEO:OpenGraph:Description"];
                ButterViewModel.ogimage = ButterResponse.Data.Fields.ogimage ?? _configuration["SEO:OpenGraph:Image"];
                
            }
            return View(ButterViewModel);

        }

    }
}
