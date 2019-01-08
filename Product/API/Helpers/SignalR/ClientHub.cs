using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;


namespace UpDiddyApi.Helpers.SignalR
{
    // public class ClientHub : Hub, IClientHub
    public class ClientHub : Hub
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        public ClientHub( IConfiguration configuration,  IDistributedCache distributedCache)
        {
            _cache = distributedCache;
            _configuration = configuration;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }
        
        // Called by client side javascript to associate a connectionId with a subscriber and verb
        public void Subscribe( string subscriberGuid, string verb)
        {
            string hubId = Context.ConnectionId;
            string cacheKey = $"{subscriberGuid}{verb}".ToLower();
            int cacheTTLInMinutes = int.Parse(_configuration["SignalR:cacheTTLInMinutes"]);
            _cache.SetString(cacheKey, hubId,  new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTTLInMinutes) });
        }
 
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

    }
}
