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
        ITokenCache cache;

        public MSALSessionCache(string userId, IDistributedCache distributedCache)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            _distributedCache = distributedCache;          
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
            _distributedCache.SetString(CacheId + "_state", state);
            SessionLock.ExitWriteLock();
        }
        public string ReadUserStateValue()
        {
            string state = string.Empty; 
            state =  _distributedCache.GetString(CacheId + "_state");    
            return state;
        }
        public void Load(ITokenCacheSerializer tokenCacheSerializer)
        {         
            byte[] blob = _distributedCache.Get(CacheId);
            if (blob != null)
            {
                tokenCacheSerializer.DeserializeMsalV3(blob);
            }         
        }

        public void Persist(ITokenCacheSerializer tokenCacheSerializer)
        {            
            // Reflect changes in the persistent store                     
            _distributedCache.Set(CacheId, tokenCacheSerializer.SerializeMsalV3());
            
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