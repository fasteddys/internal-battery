using Microsoft.Identity.Client;

using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;

namespace UpDiddy.Models
{
    public class MSALSessionCache
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string UserId = string.Empty;
        string CacheId = string.Empty; 
        IDistributedCache _distributedCache = null;
        IMemoryCache _memoryCache = null;
        ITokenCache cache;
 

        public MSALSessionCache(string userId, IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }

        public ITokenCache EnablePersistence(ITokenCache cache)
        {
            this.cache = cache;
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);          
            return cache;
        }

        public void SaveUserStateValue(string state)
        {
            SessionLock.EnterWriteLock();
            // store the state in memory cache as well as backed in redis 
            _memoryCache.Set(CacheId + "_state", state);
            _distributedCache.SetString(CacheId + "_state", state);
            SessionLock.ExitWriteLock();
        }
        public string ReadUserStateValue()
        {
            string state = string.Empty;
            // try and get the state from the memory cache first
            if (!_memoryCache.TryGetValue(CacheId + "_state", out state))
            {
                state = _distributedCache.GetString(CacheId + "_state");
                // add to the memory cache if we had to get it from redis 
                _memoryCache.Set(CacheId + "_state", state);
            }

            return state;
        }
        public void Load(ITokenCacheSerializer tokenCacheSerializer)
        {
            byte[] blob = null;
            if (!_memoryCache.TryGetValue(CacheId, out blob))
            {
                blob = _distributedCache.Get(CacheId);
                // add to memory cached if we had to get the value from redis 
                _memoryCache.Set(CacheId, blob);
            }
                
            if (blob != null)
            {
                tokenCacheSerializer.DeserializeMsalV3(blob);
            }         
        }

        public void Persist(ITokenCacheSerializer tokenCacheSerializer)
        {
            var MsalInfo = tokenCacheSerializer.SerializeMsalV3();
            // store the info in the process memory cache
            _memoryCache.Set(CacheId, MsalInfo);
            // back the memory cache with redis                   
            _distributedCache.Set(CacheId, MsalInfo);            
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load(args.TokenCache);
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                Persist(args.TokenCache);
            }
        }
    }
}