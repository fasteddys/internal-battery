using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using UpDiddy.Api;
using UpDiddyLib.Helpers;
using UpDiddyLib.Dto;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.Controllers
{
    public class BaseController : Controller
    {
        public SubscriberDto subscriber;
        protected IApi _Api = null;
        protected IConfiguration _configuration;
        protected int _maxCookieLength = -1;

        public BaseController(IApi api, IConfiguration configuration)
        {
            this._Api = api;
            _configuration = configuration;
            _maxCookieLength = int.Parse(_configuration["CareerCircle:MaxCookieLength"]);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
          

            base.OnActionExecuting(context); 
            //check for referral code and create a cookie
            if (Request != null && Request.Cookies["referrerCode"] == null)
            {
                if (!string.IsNullOrEmpty(Request.Query["referrerCode"].ToString()))
                {
                    SetCookie("referrerCode",Utils.AlphaNumeric(Request.Query["referrerCode"].ToString(), _maxCookieLength) , 262800);
                }
            }

            //cookie the user with the source             
            if (Request != null && Request.Query != null && string.IsNullOrEmpty(Request.Query["Source"].ToString()) == false )
            {              
                    SetCookie("Source", Utils.AlphaNumeric(Request.Query["Source"].ToString(), _maxCookieLength), 262800);             
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
            int MaxCookieLength = int.Parse(_configuration["CareerCircle:MaxCookieLength"]);

            value = Utils.AlphaNumeric(value, MaxCookieLength);

            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            Response.Cookies.Append(key, value, option);
        }

        public void RemoveCookie(string key)
        {
            Response.Cookies.Delete(key);
        }


        #endregion
    }
}