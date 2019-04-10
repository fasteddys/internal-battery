using System;
using System.Collections.Generic;
using System.Linq;
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
using UpDiddy.ViewModels.Components.Layout;

namespace UpDiddy.ViewComponents
{
    public class PublicSiteNavigationViewComponent : ViewComponent
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        // todo: part of refactor of API updiddy
        private IApi _Api = null;
        private IConfiguration _configuration = null;
        private IDistributedCache _cache = null;
        private IButterCMSService _butterService = null;

        public PublicSiteNavigationViewComponent(
            IApi api, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, 
            IDistributedCache cache,
            IButterCMSService butterService)
        {
            _Api = api;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _butterService = butterService;
        }

        public IViewComponentResult Invoke()
        {
            var ButterResponse =_butterService.GetResponse<CareerCirclePublicSiteNavigationViewModel<MenuItemViewModel>>("careercircle_public_site_navigation", "CareerCirclePublicSiteNavigation");
            MenuItemViewModel PublicSiteNavigation = FindDesiredNavigation(ButterResponse, "CareerCircleSiteNavigation");
            return View(PublicSiteNavigation);
        }

        private MenuItemViewModel FindDesiredNavigation(CareerCirclePublicSiteNavigationViewModel<MenuItemViewModel> CCNavigation, string NavigationLabel)
        {
            foreach(MenuItemViewModel MenuItem in CCNavigation.CareerCirclePublicSiteNavigationRoot)
            {
                if (MenuItem.Label.Equals(NavigationLabel))
                    return MenuItem;
            }

            return null;
        }


    }


}
