using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IMemoryCacheService
    {
        /// <summary>
        /// Fetches a cached object by its cache key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetCacheValue(string key);

        /// <summary>
        /// Sets/creates a cached object with a cache key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="TTLInMinutes"></param>
        void SetCacheValue<T>(string key, T value, int TTLInMinutes = 10);

        /// <summary>
        /// Clears/deletes a cached object by its cache key.
        /// </summary>
        /// <param name="key"></param>
        void ClearCacheByKey(string key);
    }
}