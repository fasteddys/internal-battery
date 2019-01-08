using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using UpDiddy.Api;
using UpDiddyLib.Helpers;
using UpDiddyLib.Dto;


namespace UpDiddy.Controllers
{

    public class BaseController : Controller
    {
        public SubscriberDto subscriber;
        protected IApi _Api  = null;

        public BaseController(IApi api)
        {
            this._Api = api;
        }

        public Guid GetSubscriberGuid()
        {
            Guid subscriberGuid;
            var objectId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (Guid.TryParse(objectId, out subscriberGuid))
                return subscriberGuid;
            else
                return Guid.Empty;
        }

        public void GetSubscriber(bool HardRefresh)
        {
 
            if (User.Identity.IsAuthenticated)
            {
                // Try to get the subscriber from session 
                string SubscriberJson = "";
                if(!HardRefresh)
                    SubscriberJson = HttpContext.Session.GetString(Constants.SubsriberSessionKey);   
                
                if ( String.IsNullOrEmpty(SubscriberJson)  )
                {
                    this.subscriber = _Api.Subscriber();
                    if ( this.subscriber != null )
                        HttpContext.Session.SetString(Constants.SubsriberSessionKey, JsonConvert.SerializeObject(this.subscriber));
                }
                else 
                    this.subscriber = JsonConvert.DeserializeObject<SubscriberDto>(SubscriberJson);
                
                // if the user is not a subscriber, create their subscriber record now
                if ( this.subscriber == null )
                {
                    this.subscriber = _Api.CreateSubscriber();
                    HttpContext.Session.SetString(Constants.SubsriberSessionKey, JsonConvert.SerializeObject(this.subscriber));
                }
                    
            }
            else
                this.subscriber = null;
            
        }
    }
}
