using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class LeadStatusFactory : FactoryBase
    {
        private const string CACHE_KEY = "GetAllLeadStatuses";
        private readonly UpDiddyDbContext _db;
        private readonly ILogger _syslog;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;

        public LeadStatusFactory(UpDiddyDbContext db, IConfiguration configuration, ILogger syslog, IDistributedCache distributedCache) :
            base(db, configuration, syslog, distributedCache)
        {
            _db = db;
            _distributedCache = distributedCache;
            _syslog = syslog;
            _configuration = configuration;
        }

        public List<LeadStatus> GetAllLeadStatuses()
        {
            var leadStatuses = GetCachedValue<List<LeadStatus>>(CACHE_KEY);

            if(leadStatuses == null)
            {
                leadStatuses = _db.LeadStatus.Where(ls => ls.IsDeleted == 0).ToList();
                SetCachedValue<List<LeadStatus>>(CACHE_KEY, leadStatuses);
            }
            return leadStatuses;
        }
    }
}
