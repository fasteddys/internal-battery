using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace UpDiddyApi.Helpers
{
    public class ClientHub : Hub
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public ClientHub(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            var wtf = Context.UserIdentifier;
            var x = Context.User.Identity.Name;
            var xx = _contextAccessor.HttpContext.User.Identity.Name;
            //await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }


        public void SendTest()
        {
            Clients.All.SendAsync("Send", "Hello World");
        }
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

    }
}
