using System;
using System.Web;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using UpDiddy.ViewModels;

namespace UpDiddy.ViewComponents
{
    public class LinkedInSyncViewComponent : ViewComponent
    {
        public static string DEFAULT_VIEW = "Default";
        public static string SYNCED_VIEW = "Synced";

        public static string SYNCING_VIEW = "Syncing";

        private readonly IHttpContextAccessor _httpContextAccessor;

        // todo: part of refactor of API updiddy
        private IApi _Api  = null;
        private IConfiguration _configuration = null;

        public LinkedInSyncViewComponent(IApi api, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _Api = api;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid SubscriberGuid, DateTime LastSyncDate, string LinkedInAvatarUrl)
        {
            // TODO JAB check various states as synced, syncing unsynced etc. 

            var responseQuery = _httpContextAccessor.HttpContext.Request.Query;
            var returnUrl = UriHelper.GetDisplayUrl(_httpContextAccessor.HttpContext.Request);
            // not null indicates user has requested a resync
            string liCode = responseQuery["code"].ToString();


            // check to see if the user has synced with linked at some point 
            LinkedInProfileDto lidto = await _Api.GetLinkedInProfileAsync();
            // if the licode is null then the user has not requested a sync return the synced view 
            if(lidto != null && string.IsNullOrEmpty(liCode) )
            {
                return View(LinkedInSyncViewComponent.SYNCED_VIEW, new LinkedInSyncViewModel {
                    SyncLink = GetLinkedInRequestAuthCodeUrl(),
                    LinkedInAvatarUrl = LinkedInAvatarUrl,
                    LastLinkedInSyncDate = LastSyncDate
                });
            }
  

            // if the use has requested a synce
            if ( liCode != null && CheckGetParams(responseQuery))
            {
                BasicResponseDto apiResponse = await _Api.SyncLinkedInAccountAsync(responseQuery["code"], returnUrl);

                // todo: perhaps display error in some way if this failed
                // if error then they will need to get another authcode and try again
                if(apiResponse.StatusCode == 1)
                    return View(LinkedInSyncViewComponent.SYNCING_VIEW, GetLinkedInRequestAuthCodeUrl());
            }

            // return the default view 
            return View(LinkedInSyncViewComponent.DEFAULT_VIEW, GetLinkedInRequestAuthCodeUrl() );
        }

        /// <summary>
        /// Checks the current requests get params to see if LinkedIn redirected to our site
        /// </summary>
        /// <param name="query">Get Query</param>
        /// <returns>boolean</returns>
        private bool CheckGetParams(IQueryCollection query) => !String.IsNullOrEmpty(query["code"]) && !String.IsNullOrEmpty(query["state"]);
    
        /// <summary>
        /// Builds URL that user will travel to give permission to Career Circle LinkedIn App
        /// and returns with auth code for API to use.
        /// </summary>
        /// <returns>string - url</returns>
        private string GetLinkedInRequestAuthCodeUrl()
        {
            UriBuilder uriBuilder = new UriBuilder("https://www.linkedin.com/oauth/v2/authorization");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["response_type"] = _configuration["LinkedIn:ResponseType"];
            query["client_id"] = _configuration["LinkedIn:ClientId"];
            // For now can only be used on home/profile since the API get the linked in URI from a app settings.
            // TODO in the future try passing the url to our API via a querystring parameter
            query["redirect_uri"] = UriHelper.GetDisplayUrl(_httpContextAccessor.HttpContext.Request);
            query["state"] = _configuration["LinkedIn:State"];
            query["scope"] = _configuration["LinkedIn:Scope"];
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
    }
}