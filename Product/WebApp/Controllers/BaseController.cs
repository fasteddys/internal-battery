using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddy.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UpDiddy.Helpers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Polly.Registry;
using System.Collections;
using Microsoft.Extensions.Caching.Distributed;

namespace UpDiddy.Controllers
{

    public class BaseController : Controller
    {
     
        public SubscriberDto subscriber;
        private ApiUpdiddy _Api  = null;
        private AzureAdB2COptions _AzureAdB2COptions = null;
        private IConfiguration _configuration = null;
        private IHttpClientFactory _HttpClientFactory = null;
        private IDistributedCache _cache = null;


        public BaseController(AzureAdB2COptions AzureAdB2COptions, IConfiguration configuration, IHttpClientFactory httpClientFactory, IDistributedCache cache)
        {
            _AzureAdB2COptions = AzureAdB2COptions;
            _configuration = configuration;
            _HttpClientFactory = httpClientFactory;
            _cache = cache;    
        }

        // todo: can we make this a singleton or a part of DI opposed to creating one in each controller
         public ApiUpdiddy API {
            get
            {
                if (_Api != null)
                    return _Api;
                else
                {
                    _Api = new ApiUpdiddy(_AzureAdB2COptions, this.HttpContext, _configuration, _HttpClientFactory, _cache);
                    return _Api;
                }
            }
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
                    this.subscriber = API.Subscriber(Guid.Parse(objectId));
                    if ( this.subscriber != null )
                    HttpContext.Session.SetString(Constants.SubsriberSessionKey, JsonConvert.SerializeObject(this.subscriber));
                }
                else 
                    this.subscriber = JsonConvert.DeserializeObject<SubscriberDto>(SubscriberJson);
                
                // if the user is not a subscriber, create their subscriber record now
                if ( this.subscriber == null )
                {
                    this.subscriber = API.CreateSubscriber(objectId, email);
                    HttpContext.Session.SetString(Constants.SubsriberSessionKey, JsonConvert.SerializeObject(this.subscriber));
                }
                    
            }
            else
                this.subscriber = null;
            
        }
    }
}
