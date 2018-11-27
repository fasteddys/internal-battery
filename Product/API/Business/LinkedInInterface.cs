using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UpDiddyLib.Helpers;
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;
using UpDiddy.Helpers;

namespace UpDiddyApi.Business
{
    public class LinkedInInterface : BusinessVendorBase
    {

        #region Class


        public LinkedInInterface( UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            _db = db;
            _mapper = mapper;
            // TODO: CRITICAL, Azure Key Vault does NOT permit colons. See https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.1  
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _apiBaseUri = _configuration["LinkedIn:ApiUrl"];
            _syslog = new SysLog(configuration, sysemail, serviceProvider);
        }

        #endregion

        #region Acquire Profile Data 

        public int ImportProfile(Guid SubscriberGuid )
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.ImportProfile started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid.ToString()}");

                int rVal = 0;
                LinkedInToken lit = _db.LinkedInToken
                   .Where(t => t.IsDeleted == 0 && t.SubscriberGuid == SubscriberGuid)
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
                         .Where(t => t.IsDeleted == 0 && t.SubscriberGuid == SubscriberGuid && t.ProfileSource == Constants.LinkedInProfile)
                     .FirstOrDefault();

                    bool spsExists = sps != null;
                    // Create a new profile staging record 
                    if (!spsExists)
                    {
                        sps = new SubscriberProfileStagingStore();
                        sps.CreateDate = DateTime.Now;
                        sps.ModifyGuid = Guid.NewGuid();
                        sps.SubscriberGuid = SubscriberGuid;
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

        public int AcquireBearerToken(Guid SubscriberGuid, string Code, LinkedInToken lit  )
        {
            try
            {
                int rval = 0;
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.AcquireAccessToken started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid.ToString()}");
   
                string redirectUrl = _configuration["LinkedIn:RedirectUrl"];
                string clientId = _configuration["LinkedIn:ClientId"];
                string clientSecret = _configuration["LinkedIn:ClientSecret"];

                // TODO deal with LinkedIn different API versions
                string Url = $"https://www.linkedin.com/oauth/v2/accessToken?grant_type=authorization_code&code={Code}&redirect_uri={redirectUrl}&client_id={clientId}&client_secret={clientSecret}";
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
                        lit = new LinkedInToken();
                        lit.CreateDate = DateTime.Now;
                        lit.CreateGuid = Guid.NewGuid();
                        lit.SubscriberGuid = SubscriberGuid;
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


        public int SyncProfile(Guid SubscriberGuid, string Code)
        {
            try
            {
                int rval = 0;
                _syslog.Log(LogLevel.Information, $"***** LinkedInInterace.SyncProfile started at: {DateTime.UtcNow.ToLongDateString()} for subscriber {SubscriberGuid.ToString()}");
                // Get the Enrollment Object 
                LinkedInToken lit = _db.LinkedInToken
                     .Where(t => t.IsDeleted == 0 && t.SubscriberGuid == SubscriberGuid)
                     .FirstOrDefault();

                // if the user does not have a linked in token or their bearer token has expired get them a bearer token
                if (lit == null || lit.AccessTokenExpiry < DateTime.Now)
                    rval = AcquireBearerToken(SubscriberGuid, Code, lit);

                if ( rval == (int) ProfileDataStatus.Acquired )
                    rval = ImportProfile(SubscriberGuid);
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
