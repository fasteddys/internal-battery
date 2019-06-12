using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Services
{
    public interface ICacheService
    {
        bool SetCachedValue<T>(string CacheKey, T Value);
        bool SetCachedValue<T>(string CacheKey, T Value, DateTimeOffset expiryTime);
        T GetCachedValue<T>(string CacheKey);
        bool RemoveCachedValue<T>(string CacheKey);
        Task<T> GetSetCachedValueAsync<T>(string CacheKey, Func<Task<T>> func, DateTimeOffset expiryTime);
    }
}
