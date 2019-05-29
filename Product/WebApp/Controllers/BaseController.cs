using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using UpDiddy.Api;
using UpDiddyLib.Helpers;
using UpDiddyLib.Dto;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UpDiddy.Controllers
{
    public class BaseController : Controller
    {
        public SubscriberDto subscriber;
        protected IApi _Api = null;

        public BaseController(IApi api)
        {
            this._Api = api;          
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            //check for referral code and create a cookie
            if (Request != null && Request.Cookies["referrerCode"] == null)
            {
                if (!string.IsNullOrEmpty(Request.Query["referrerCode"].ToString()))
                {
                    SetCookie("referrerCode", Request.Query["referrerCode"].ToString(), 262800);
                }
            }
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

        #region Cookie
        /// <summary>
        /// Set Cookie
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireTime"></param>
        public void SetCookie(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            Response.Cookies.Append(key, value, option);
        }
        #endregion
    }
}