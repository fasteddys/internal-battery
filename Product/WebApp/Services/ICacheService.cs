using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Services
{
    public interface ICacheService
    {
        bool SetCachedValue<T>(string CacheKey, T Value);
        T GetCachedValue<T>(string CacheKey);
        bool RemoveCachedValue<T>(string CacheKey);
    }
}
