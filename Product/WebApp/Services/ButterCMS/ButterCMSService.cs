using ButterCMS;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Services.ButterCMS
{
    public class ButterCMSService : IButterCMSService
    {
        private ICacheService _cacheService = null;
        private IConfiguration _configuration = null;

        public ButterCMSService(ICacheService cacheService, IConfiguration configuration)
        {
            _cacheService = cacheService;
            _configuration = configuration;
        }

        public T GetResponse<T>(string ApiSlug, string CacheKey) where T : class
        {
            
            var CachedButterResponse = _cacheService.GetCachedValue<T>(CacheKey);
            if (CachedButterResponse == null)
            {
                var butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
                CachedButterResponse = butterClient.RetrieveContentFields<T>(new string[1] { ApiSlug });
                _cacheService.SetCachedValue(CacheKey, CachedButterResponse);
            }
            
            return CachedButterResponse;


        }


    }
}
