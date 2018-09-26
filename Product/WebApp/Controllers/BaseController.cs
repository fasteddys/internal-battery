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


namespace UpDiddy.Controllers
{

    public class BaseController : Controller
    {
        public ApiUpdiddy API;
        public SubscriberDto subscriber;

        public BaseController(AzureAdB2COptions AzureAdB2COptions, IConfiguration configuration)
        {
            this.API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, configuration);
            
            
        }

        public void setCurrentClientGuid()
        {
            string objectId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            this.subscriber = API.Subscriber(Guid.Parse(objectId));
           
        }
    }
}
