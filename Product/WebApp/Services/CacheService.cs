﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Services
{
    public class CacheService : ICacheService
    {
        private IDistributedCache _cache = null;
        private IConfiguration _configuration = null;

        public CacheService(IDistributedCache cache, IConfiguration conifguration)
        {
            _cache = cache;
            _configuration = conifguration;
        }

        public T GetCachedValue<T>(string CacheKey)
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

        public bool SetCachedValue<T>(string CacheKey, T Value)
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

        public bool RemoveCachedValue<T>(string CacheKey)
        {
            try
            {
                _cache.Remove(CacheKey);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
