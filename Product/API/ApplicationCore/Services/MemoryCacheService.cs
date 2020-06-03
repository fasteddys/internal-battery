using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class MemoryCacheService : IMemoryCacheService
    {
        
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

       public object GetCacheValue(string key)
       {
           return _memoryCache.Get(key);
       }

       public void SetCacheValue<T>( string key, T value, int TTLInMinutes = 10 )
       {
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now.AddMinutes(TTLInMinutes));
            _memoryCache.Set<T>(key, value, offset);
       }

        public void ClearCacheByKey(string key)
        {
            _memoryCache.Remove(key);
        }

    }
}
