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
                Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                this.subscriber = _Api.Subscriber(subscriberGuid, HardRefresh);
                if(this.subscriber.CampaignOffer != null)
                    HttpContext.Session.SetString("CampaignOffers", this.subscriber.CampaignOffer);
                    
            }
            else
                this.subscriber = null;
            
        }
    }
}
