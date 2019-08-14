using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace UpDiddy.ExceptionHandling
{
    public class CCExceptionFilter : IExceptionFilter
    {

        public void OnException(ExceptionContext context)
        { 
            var exceptionType = context.Exception.GetType();         
            if (exceptionType == typeof(MsalUiRequiredException))
            {
                context.ExceptionHandled = true;
                context.HttpContext.Response.Redirect("/Session/Signout");
            }                     
        }

    }

}
