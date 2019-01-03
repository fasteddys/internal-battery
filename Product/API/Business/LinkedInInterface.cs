using AutoMapper;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;
using UpDiddy.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Workflow;
using Hangfire;
using UpDiddyApi.Business.Factory;

namespace UpDiddyApi.Business
{
    public class LinkedInInterface : BusinessVendorBase
    {

        #region Class

        public LinkedInInterface(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider, ILogger<LinkedInInterface> logger)
        {
            _db = db;
            _mapper = mapper;
            // TODO: CRITICAL, Azure Key Vault does NOT permit colons. See https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.1  
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _apiBaseUri = _configuration["LinkedIn:ApiUrl"];
            _syslog = logger;
        }
        #endregion

        #region Acquire Profile Data 

        public int ImportProfile(Guid subscriberGuid )
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.ImportProfile started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {subscriberGuid.ToString()}");

                int rVal = 0;
                
                // Get the user linked in token 
                LinkedInToken lit = LinkedInTokenFactory.GetBySubcriber(_db, subscriberGuid);

                if (lit == null)
                    return (int)ProfileDataStatus.AccountNotFound;

                var ResponseJson = string.Empty;
                // Call linkedin to acquire user's profile data 
                HttpResponseMessage response = _GetUserProfileData(lit, ref ResponseJson);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Add acquired linked on profile data to the profile data staging store 
                    SubscriberProfileStagingStoreFactory.StoreProfileData(_db,subscriberGuid, ResponseJson);
                    // Kick off  job to import the subscriber's profile data file from the staging store 
                    BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileData(subscriberGuid));
                    rVal =  (int)ProfileDataStatus.Acquired;
                }
                else
                {
                    int _StatusCode = (int)response.StatusCode;
                    rVal =  (int)ProfileDataStatus.AcquistionError;
                }
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.ImportProfile completed at: {DateTime.UtcNow.ToLongDateString()}");
                return rVal;
            }
            catch ( Exception e)
            {
                _syslog.Log(LogLevel.Error, "LinkedInInterace:ImportProfile threw an exception -> " + e.Message);
                return (int)ProfileDataStatus.AcquistionError;
            }
     
        }


        private HttpResponseMessage _GetUserProfileData(LinkedInToken lit, ref string ResponseJson)
        {

            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + _configuration["LinkedIn:UserInfoUrl"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lit.AccessToken);
            HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(request));
            ResponseJson = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());

            return response;
        }

        #endregion


        #region Acquire Bearer Token

        public int AcquireBearerToken(Guid subscriberGuid, string authCode, string returnUrl, LinkedInToken lit  )
        {
            try
            {
                int rval = 0;
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.AcquireAccessToken started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {subscriberGuid.ToString()}");

                var ResponseJson = string.Empty;
                // Call linkedin to get the bearer token asscoiated with the authorization code 
                HttpResponseMessage response = _GetBearerToken(authCode, returnUrl, ref ResponseJson);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                    string accessToken = ResponseObject.access_token;
                    long expires_in_seconds = ResponseObject.expires_in;
                    // Create or update user's lit token
                    LinkedInTokenFactory.StoreToken(_db, lit, subscriberGuid, accessToken, expires_in_seconds);
                    rval = (int)ProfileDataStatus.Acquired;
                }
                else
                    rval =  (int)ProfileDataStatus.AcquistionError;

                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.AcquireAccessToken completed at: {DateTime.UtcNow.ToLongDateString()}");
                return rval;
            }
            catch ( Exception e)
            {                
                    _syslog.Log(LogLevel.Error, "LinkedInInterace:AcquireAccessToken threw an exception -> " + e.Message);
                    return (int)ProfileDataStatus.AcquistionError;
            }           
        }


        private HttpResponseMessage _GetBearerToken(string code, string returnUrl, ref string ResponseJson)
        {

            string clientId = _configuration["LinkedIn:ClientId"];
            string clientSecret = _configuration["LinkedIn:ClientSecret"];

            // Remove the querystring portion of the url so it will pass linkedIn's redirect uri validation.
            // The redirect uri must case match a uri that is specified for the app on linkedIn's developer site
            returnUrl = Utils.RemoveQueryStringFromUrl(returnUrl);

            // TODO deal with LinkedIn different API versions
            string Url = $"https://www.linkedin.com/oauth/v2/accessToken?grant_type=authorization_code&code={code}&redirect_uri={returnUrl}&client_id={clientId}&client_secret={clientSecret}";
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Url);
            HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(request));
            ResponseJson = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());

            return response;
        }

        #endregion


        public int SyncProfile(Guid subscriberGuid, string code, string returnUrl)
        {
            try
            {
                int rval = 0;
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.SyncProfile started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {subscriberGuid.ToString()}"); 

                // Get the linked in bearer token for the user 
                LinkedInToken lit = LinkedInTokenFactory.GetBySubcriber(_db, subscriberGuid);

                // If the user does not have a linked in token or their bearer token has expired get them a bearer token
                if (lit == null || lit.AccessTokenExpiry < DateTime.Now)
                    rval = AcquireBearerToken(subscriberGuid, code, returnUrl, lit);

                // Import the user's profile data from linkein
                if ( rval == (int) ProfileDataStatus.Acquired )
                    rval = ImportProfile(subscriberGuid);
                else
                    _syslog.Log(LogLevel.Error, "LinkedInInterace:SyncProfile Import proile returned an invalid status " + rval.ToString() );                

                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.SyncProfile completed at: {DateTime.UtcNow.ToLongDateString()}");
                return rval;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, "LinkedInInterace:SyncProfile threw an exception -> " + e.Message);
                return (int)ProfileDataStatus.AcquistionError;
            }
        }

    }

}
