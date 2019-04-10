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

        public T GetResponse<T>(string ApiSlug, int Levels) where T : class
        {
            
            var CachedButterResponse = _cacheService.GetCachedValue<T>(ApiSlug);
            try
            {
                if (CachedButterResponse == null)
                {
                    var butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
                    CachedButterResponse = butterClient.RetrieveContentFields<T>(new string[1] { ApiSlug }, new Dictionary<string, string> { { "levels", Levels.ToString() } });
                    _cacheService.SetCachedValue(ApiSlug, CachedButterResponse);
                }
            }
            catch(ContentFieldObjectMismatchException Exception)
            {
                if (CachedButterResponse == null)
                    return null;

            }
            
            return CachedButterResponse;


        }


    }
}
