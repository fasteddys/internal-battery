using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using UpDiddy.Api;
using UpDiddy.Controllers;
using UpDiddyLib.Helpers;

namespace UpDiddy.Authentication
{
    /// <summary>
    /// If a controller method which inherits from BaseController is decorated with this attribute, an attempt is made
    /// to retrieve the subscriber object and load it for use within the action method being executed. If a subscriber
    /// cannot be found and the subscriber is required, the request is redirected to the session signin controller method.
    /// </summary>
    public class LoadSubscriber : ActionFilterAttribute, IAsyncActionFilter
    {
        bool _isHardRefresh = false;
        bool _isSubscriberRequired = false;
        public LoadSubscriber(bool isHardRefresh, bool isSubscriberRequired)
        {
            this._isHardRefresh = isHardRefresh;
            this._isSubscriberRequired = isSubscriberRequired;
        }

        /// <summary>
        /// This method is evaluated before any code in a controller method is evaluated.
        /// </summary>
        /// <param name="filterContext"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
        {
            bool isSubscriberExists = false;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                Guid subscriberGuid = Guid.Parse(filterContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (subscriberGuid != null)
                {
                    // retrieve the UpDiddyApi using the DependencyInjection extension method
                    var api = filterContext.HttpContext.RequestServices.GetService<IApi>();

                    var subscriber = await api.SubscriberAsync(subscriberGuid, this._isHardRefresh);
                    if (subscriber != null)
                    {
                        if (subscriber.CampaignOffer != null)
                            filterContext.HttpContext.Session.SetString("CampaignOffers", subscriber.CampaignOffer);
                        // load the subscriber property if the controller in use inherits from BaseController
                        if (Utils.IsSubclassOfRawGeneric(typeof(BaseController), filterContext.Controller.GetType()))
                        {
                            var controller = (BaseController)filterContext.Controller;
                            controller.subscriber = subscriber;
                            isSubscriberExists = true;
                        }
                    }
                }
            }
            // redirect to the sign-in page if the subscriber is required and it could not be loaded successfully
            if (this._isSubscriberRequired && !isSubscriberExists)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Session",
                            action = "SignIn"
                        }
                    )
                );
            }
            else
            {
                var resultContext = await next();
            }
        }
    }
}