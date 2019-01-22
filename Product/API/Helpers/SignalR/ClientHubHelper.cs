using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.SignalR
{
    public class ClientHubHelper
    {
        private IHubContext<ClientHub> _hubContext;
        private IDistributedCache _cache;
        public ClientHubHelper(IHubContext<ClientHub> hubContext, IDistributedCache cache)
        {
            _hubContext = hubContext;
            _cache = cache;
        }

 
        public void CallClient(Guid? subscriberGuid, string verb, string data)
        {
            string cacheKey = $"{subscriberGuid}{verb}".ToLower();
            string hubId = _cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(hubId))
            _hubContext.Clients.Client(hubId).SendAsync(verb, data);
        }



    }
}
