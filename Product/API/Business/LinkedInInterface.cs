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
                LinkedInToken lit = _db.LinkedInToken
                          .Include(l => l.Subscriber)
                          .Where(l => l.IsDeleted == 0 && l.Subscriber.SubscriberGuid == subscriberGuid)
                          .FirstOrDefault();

                if (lit == null)
                    return (int)ProfileDataStatus.AccountNotFound;

                HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + _configuration["LinkedIn:UserInfoUrl"]);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lit.AccessToken);
                HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(request));
                var ResponseJson = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    SubscriberProfileStagingStore sps = _db.SubscriberProfileStagingStore
                         .Include( s=> s.Subscriber)
                         .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid && s.ProfileSource == Constants.LinkedInProfile)
                     .FirstOrDefault();

                    bool spsExists = sps != null;
                    // Create a new profile staging record 
                    if (!spsExists)
                    {

                        var Subscriber = _db.Subscriber
                        .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                          .FirstOrDefault();

                        if (Subscriber == null)
                            throw new Exception($"LinkedInInterface:ImportProfile Subscriber not found for {subscriberGuid}");

                        sps = new SubscriberProfileStagingStore();
                        sps.CreateDate = DateTime.Now;
                        sps.ModifyGuid = Guid.NewGuid();
                        sps.SubscriberId = Subscriber.SubscriberId;
                        sps.ProfileSource = Constants.LinkedInProfile;
                        sps.IsDeleted = 0;
                        sps.ProfileFormat = Constants.DataFormatJson;
                    }
                    sps.ModifyDate = DateTime.Now;
                    sps.ProfileData = ResponseJson;
                    sps.Status = (int)ProfileDataStatus.Acquired;
                    if (!spsExists)
                        _db.SubscriberProfileStagingStore.Add(sps);
                    _db.SaveChanges();

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

        #endregion


        #region Acquire Bearer Token

        public int AcquireBearerToken(Guid subscriberGuid, string code, string returnUrl, LinkedInToken lit  )
        {
            try
            {
                int rval = 0;
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.AcquireAccessToken started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {subscriberGuid.ToString()}");

                // Remove the querystring portion of the url so it will pass linkedIn's redirect uri validation.
                // The redirect uri must case match a uri that is specified for the app on linkedIn's developer site
                returnUrl = Utils.RemoveQueryStringFromUrl(returnUrl);
          
                string clientId = _configuration["LinkedIn:ClientId"];
                string clientSecret = _configuration["LinkedIn:ClientSecret"];

                // TODO deal with LinkedIn different API versions
                string Url = $"https://www.linkedin.com/oauth/v2/accessToken?grant_type=authorization_code&code={code}&redirect_uri={returnUrl}&client_id={clientId}&client_secret={clientSecret}";
                HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Url);
                HttpResponseMessage response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(request));
                var ResponseJson = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                    string accessToken = ResponseObject.access_token;
                    long expires_in_seconds = ResponseObject.expires_in;

                    bool litExists = (lit != null);
                    if (!litExists)
                    {

                        var Subscriber = _db.Subscriber
                            .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                            .FirstOrDefault();

                        if (Subscriber == null)
                            throw new Exception($"LinkedInInterface:AcquireBearerToken Subscriber not found for {subscriberGuid}");

                        lit = new LinkedInToken();
                        lit.CreateDate = DateTime.Now;
                        lit.CreateGuid = Guid.NewGuid();
                        lit.SubscriberId = Subscriber.SubscriberId;
                        lit.IsDeleted = 0;
                    }
                    lit.ModifyDate = DateTime.Now;
                    lit.AccessToken = accessToken;
                    lit.AccessTokenExpiry = DateTime.Now.AddSeconds(expires_in_seconds);

                    if (!litExists)
                        _db.LinkedInToken.Add(lit);
                    _db.SaveChanges();

                    rval = (int)ProfileDataStatus.Acquired;
                }
                else
                {
                    int _StatusCode = (int)response.StatusCode;
                    rval =  (int)ProfileDataStatus.AcquistionError;
                }
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.AcquireAccessToken completed at: {DateTime.UtcNow.ToLongDateString()}");
                return rval;
            }
            catch ( Exception e)
            {
                
                    _syslog.Log(LogLevel.Error, "LinkedInInterace:AcquireAccessToken threw an exception -> " + e.Message);
                    return (int)ProfileDataStatus.AcquistionError;
            }           
        }


        #endregion


        public int SyncProfile(Guid subscriberGuid, string code, string returnUrl)
        {
            try
            {
                int rval = 0;
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.SyncProfile started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {subscriberGuid.ToString()}");
                // Get the Enrollment Object 
                LinkedInToken lit = _db.LinkedInToken
                     .Include( l => l.Subscriber)
                     .Where(l => l.IsDeleted == 0 && l.Subscriber.SubscriberGuid == subscriberGuid)
                     .FirstOrDefault();

                // if the user does not have a linked in token or their bearer token has expired get them a bearer token
                if (lit == null || lit.AccessTokenExpiry < DateTime.Now)
                    rval = AcquireBearerToken(subscriberGuid, code, returnUrl, lit);

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
