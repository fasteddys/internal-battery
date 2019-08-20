using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace UpDiddy.ExceptionHandling
{
    public class CCExceptionFilter : ExceptionFilterAttribute
    {

        IMemoryCache _memoryCache = null;
        IConfiguration _config = null;
        public CCExceptionFilter(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _config = configuration;

        }


        public override void OnException(ExceptionContext context)
        { 
            var exceptionType = context.Exception.GetType();         
            if (exceptionType == typeof(MsalUiRequiredException) )
            {
                // cache the page the user was trying to access when the MsalUIRequiredException occured 
                // so they can be redirected after logging back in
                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                string CacheKey = $"{userId}MsalUiRequiredRedirect";
                int MsalRedirectTTLInSeconds = int.Parse(_config["CareerCircle:MsalRedirectTTLInSeconds"]);
                _memoryCache.Set<String>(CacheKey, context.HttpContext.Request.Path,  DateTime.Now.AddSeconds(MsalRedirectTTLInSeconds).TimeOfDay );
                context.ExceptionHandled = true;
                context.HttpContext.Response.Redirect("/Session/AuthRequired");
            }                     
            
        }

    }

}
