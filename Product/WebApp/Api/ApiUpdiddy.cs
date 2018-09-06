using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UpDiddy.Helpers;

namespace UpDiddy.Api
{
    public class ApiUpdiddy : ApiHelperMsal
    {
    
        public ApiUpdiddy(AzureAdB2COptions Options, HttpContext Context, IConfiguration conifguration) {

            AzureOptions = Options;
            HttpContext = Context;
            _configuration = conifguration;
            // Set the base URI for API calls 
            _ApiBaseUri = _configuration["Api:ApiUrl"];

        }

        public string Hello()
        {
            return Get<string>("hello", true);
        }
    }
}


