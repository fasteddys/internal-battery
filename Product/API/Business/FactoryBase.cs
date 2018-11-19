using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Business
{
    public class FactoryBase
    {

        protected readonly UpDiddyDbContext _db;
        protected readonly IConfiguration _configuration;
        protected readonly ISysLog _syslog;
        protected readonly IDistributedCache _cache;

        public FactoryBase(UpDiddyDbContext db, IConfiguration configuration, ISysEmail sysemail, IServiceProvider serviceProvider, IDistributedCache distributedCache)
        {
            _db = db;
            _configuration = configuration;
            _syslog = new SysLog(configuration, sysemail, serviceProvider);
            _cache = distributedCache;
        }


        protected bool SetCachedValue<T>(string CacheKey, T Value)
        {
            try
            {
                int CacheTTL = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
                string newValue = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                _cache.SetString(CacheKey, newValue, new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddHours(CacheTTL) });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected T GetCachedValue<T>(string CacheKey)
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
    }
}
