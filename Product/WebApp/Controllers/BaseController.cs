﻿using Microsoft.Extensions.Localization;
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

namespace UpDiddy.Controllers
{

    public class BaseController : Controller
    {
     
        public SubscriberDto subscriber;

        public BaseController(AzureAdB2COptions AzureAdB2COptions, IConfiguration configuration)
        {
            this.API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, configuration);                        
        }

        public ApiUpdiddy API { get;  }        

        public void GetSubscriber()
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
                string SubscriberJson = HttpContext.Session.GetString(Constants.SubsriberSessionKey);                
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
