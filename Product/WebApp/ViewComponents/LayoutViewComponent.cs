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
using UpDiddy.ViewModels.Components.Layout;

namespace UpDiddy.ViewComponents
{
    public class LayoutViewComponent : ViewComponent
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        // todo: part of refactor of API updiddy
        private IApi _Api = null;
        private IConfiguration _configuration = null;
        private IDistributedCache _cache = null;

        public LayoutViewComponent(IApi api, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IDistributedCache cache)
        {
            _Api = api;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public IViewComponentResult Invoke()
        {
            var butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
            MenuRootViewModel<MenuItemViewModel> Items = butterClient.RetrieveContentFields<MenuRootViewModel<MenuItemViewModel>>(new string[1] { "menu_item" });
            
            return View(Items);
        }

        #region Cache Access Methods

        private bool SetCachedValue<T>(string CacheKey, T Value)
        {
            try
            {
                int CacheTTL = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
                string newValue = JsonConvert.SerializeObject(Value);
                _cache.SetString(CacheKey, newValue, new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(CacheTTL) });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private T GetCachedValue<T>(string CacheKey)
        {
            try
            {
                string existingValue = _cache.GetString(CacheKey);
                if (string.IsNullOrEmpty(existingValue))
                    return (T)Convert.ChangeType(null, typeof(T));
                else
                {
                    T rval = JsonConvert.DeserializeObject<T>(existingValue);
                    return rval;
                }
            }
            catch (Exception ex)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }

        #endregion


    }


}
