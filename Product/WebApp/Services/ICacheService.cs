using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Services
{
    public interface ICacheService
    {
        Task<bool> SetCachedValueAsync<T>(string CacheKey, T Value);
        Task<bool> SetCachedValueAsync<T>(string CacheKey, T Value, DateTimeOffset expiryTime);
        Task<T> GetCachedValueAsync<T>(string CacheKey);
        Task<bool> RemoveCachedValueAsync(string CacheKey);
        Task<T> GetSetCachedValueAsync<T>(string CacheKey, Func<Task<T>> func, DateTimeOffset expiryTime);
    }
}
