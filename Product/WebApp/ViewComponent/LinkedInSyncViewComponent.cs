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

namespace UpDiddy.ViewComponents
{
    public class LinkedInSyncViewComponent : ViewComponent
    {
        public static string DEFAULT_VIEW = "Default";
        public static string SYNCED_VIEW = "Synced";

        public static string SYNCING_VIEW = "Syncing";

        private readonly IHttpContextAccessor _httpContextAccessor;

        // todo: part of refactor of API updiddy
        private ApiUpdiddy _Api  = null;
        private AzureAdB2COptions _AzureAdB2COptions = null;
        private IConfiguration _configuration = null;
        private IHttpClientFactory _HttpClientFactory = null;
        private IDistributedCache _cache = null;

        public LinkedInSyncViewComponent(IHttpContextAccessor httpContextAccessor, IOptions<AzureAdB2COptions> azureAdB2COptions, IConfiguration configuration, IHttpClientFactory httpClientFactory, IDistributedCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
            _HttpClientFactory = httpClientFactory;
            _cache = cache; 

        }

        // todo: replace with DI when API is refactored in BaseController
        public ApiUpdiddy API {
            get
            {
                if (_Api != null)
                    return _Api;
                else
                {
                    _Api = new ApiUpdiddy(_AzureAdB2COptions, _httpContextAccessor.HttpContext,  _configuration, _HttpClientFactory, _cache);
                    return _Api;
                }
            }
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid SubscriberGuid)
        {
            var responseQuery = _httpContextAccessor.HttpContext.Request.Query;
            // if true then send code to API
            if(CheckGetParams(responseQuery))
            {
                BasicResponseDto apiResponse = API.SyncLinkedInAccount(SubscriberGuid, responseQuery["code"]);

                // todo: perhaps display error in some way if this failed
                // if error then they will need to get another authcode and try again
                if(int.Parse(apiResponse.StatusCode) == 200)
                    return View(LinkedInSyncViewComponent.SYNCING_VIEW);
            }

            return View(LinkedInSyncViewComponent.DEFAULT_VIEW, GetLinkedInRequestAuthCodeUrl());
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
            query["response_type"] = "code";
            query["client_id"] = "78v0k3ylkrh325";
            query["redirect_uri"] = UriHelper.GetDisplayUrl(_httpContextAccessor.HttpContext.Request);
            query["state"] = "random123"; // todo: CSRF check per LinkedIn recommendation
            query["scope"] = "r_basicprofile";
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }
    }
}