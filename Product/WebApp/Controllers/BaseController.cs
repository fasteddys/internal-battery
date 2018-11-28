using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using UpDiddy.Api;
using UpDiddy.Helpers;
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
    
        public void GetSubscriber(bool HardRefresh)
        {
 
            if (User.Identity.IsAuthenticated)
            {
                Claim emailClaim = User.Claims.Where(c => c.Type == "emails").FirstOrDefault();
                string email = string.Empty;
                if (emailClaim != null)
                    email = emailClaim.Value;
                else
                    throw new Exception("Unable to locate email claim");

                string objectId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                // Try to get the subscriber from session 

                string SubscriberJson = "";
                if(!HardRefresh)
                    SubscriberJson = HttpContext.Session.GetString(Constants.SubsriberSessionKey);   
                
                if ( String.IsNullOrEmpty(SubscriberJson)  )
                {
                    this.subscriber = _Api.Subscriber(Guid.Parse(objectId));
                    if ( this.subscriber != null )
                    HttpContext.Session.SetString(Constants.SubsriberSessionKey, JsonConvert.SerializeObject(this.subscriber));
                }
                else 
                    this.subscriber = JsonConvert.DeserializeObject<SubscriberDto>(SubscriberJson);
                
                // if the user is not a subscriber, create their subscriber record now
                if ( this.subscriber == null )
                {
                    this.subscriber = _Api.CreateSubscriber(objectId, email);
                    HttpContext.Session.SetString(Constants.SubsriberSessionKey, JsonConvert.SerializeObject(this.subscriber));
                }
                    
            }
            else
                this.subscriber = null;
            
        }
    }
}
