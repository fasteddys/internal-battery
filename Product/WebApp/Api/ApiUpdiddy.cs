using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using UpDiddy.Models;
using UpDiddyLib.Dto;
using UpDiddy.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;

namespace UpDiddy.Api
{
    public class ApiUpdiddy : IApi
    {
        protected IConfiguration _configuration { get; set; }
        protected string _ApiBaseUri = String.Empty;
        protected IHttpClientFactory _HttpClientFactory { get; set; }
        public AzureAdB2COptions AzureOptions { get; set; }
        private IHttpContextAccessor _contextAccessor { get; set; }

        public IDistributedCache _cache { get; set; }

        #region Constructor
        public ApiUpdiddy(IOptions<AzureAdB2COptions> azureAdB2COptions, IHttpContextAccessor contextAccessor, IConfiguration conifguration, IHttpClientFactory httpClientFactory, IDistributedCache cache) {

            AzureOptions = azureAdB2COptions.Value;
            _contextAccessor = contextAccessor;
            _configuration = conifguration;
            // Set the base URI for API calls 
            _ApiBaseUri = _configuration["Api:ApiUrl"];
            _HttpClientFactory = httpClientFactory;
            _cache = cache;
 
        }
        #endregion

        #region Public Cached Methods 
        public IList<TopicDto> Topics()
        {
            string cacheKey = "Topics";
            IList<TopicDto> rval = GetCachedValue<IList<TopicDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _Topics();
                SetCachedValue<IList<TopicDto>>(cacheKey, rval);
            }
            return rval;
        }


        public TopicDto TopicById(int TopicId)
        {
            string cacheKey = $"TopicById{TopicId}";
            TopicDto rval = GetCachedValue<TopicDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _TopicById(TopicId);
                SetCachedValue<TopicDto>(cacheKey, rval);
            }
            return rval;
        }

        public TopicDto TopicBySlug(string TopicSlug)
        {
            string cacheKey = $"TopicBySlug{TopicSlug}";
            TopicDto rval = GetCachedValue<TopicDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _TopicBySlug(TopicSlug);
                SetCachedValue<TopicDto>(cacheKey, rval);
            }
            return rval;            
        }

        public IList<CourseDto> getCousesByTopicSlug(string TopicSlug)
        {
            string cacheKey = $"getCousesByTopicSlug{TopicSlug}";
            IList<CourseDto> rval = GetCachedValue<IList<CourseDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _getCousesByTopicSlug(TopicSlug);
                SetCachedValue<IList<CourseDto>>(cacheKey, rval);
            }
            return rval;
        }

        public CourseDto Course(string CourseSlug)
        {
            string cacheKey = $"Course{CourseSlug}";
            CourseDto rval = GetCachedValue<CourseDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _Course(CourseSlug);
                SetCachedValue<CourseDto>(cacheKey, rval);
            }
            return rval;

        }

        public CourseDto CourseByGuid(Guid CourseGuid)
        {

            string cacheKey = $"CourseByGuid{CourseGuid}";
            CourseDto rval = GetCachedValue<CourseDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _CourseByGuid(CourseGuid);
                SetCachedValue<CourseDto>(cacheKey, rval);
            }
            return rval;
        }
 
        public WozTermsOfServiceDto GetWozTermsOfService()
        {
            string cacheKey = $"GetWozTermsOfService";
            WozTermsOfServiceDto rval = GetCachedValue<WozTermsOfServiceDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetWozTermsOfService();
                SetCachedValue<WozTermsOfServiceDto>(cacheKey, rval);
            }
            return rval;
        }
        

        public WozCourseProgressDto GetCurrentCourseProgress(Guid SubscriberGuid, Guid EnrollmentGuid)
        {
            string cacheKey = $"GetCurrentCourseProgress{SubscriberGuid}{EnrollmentGuid}";
            WozCourseProgressDto rval = GetCachedValue<WozCourseProgressDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetCurrentCourseProgress(SubscriberGuid, EnrollmentGuid);
                SetCachedValue<WozCourseProgressDto>(cacheKey, rval);
            }
            return rval;
        }
        
        public IList<CountryDto> GetCountries()
        {
            string cacheKey = $"GetCountries";
            IList<CountryDto> rval = GetCachedValue<IList<CountryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetCountries();
                SetCachedValue<IList<CountryDto>>(cacheKey, rval);
            }
            return rval;
        }
        public IList<StateDto> GetStatesByCountry(Guid? countryGuid)
        {
            string cacheKey = $"GetStatesByCountry{countryGuid}";
            IList<StateDto> rval = GetCachedValue<IList<StateDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetStatesByCountry(countryGuid);
                SetCachedValue<IList<StateDto>>(cacheKey, rval);
            }
            return rval;
        }

        public CourseVariantDto GetCourseVariant(Guid courseVariantGuid)
        {
            string cacheKey = $"GetCourseVariant{courseVariantGuid}";
            CourseVariantDto rval = GetCachedValue<CourseVariantDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetCourseVariant(courseVariantGuid);
                SetCachedValue<CourseVariantDto>(cacheKey, rval);
            }
            return rval;
        }

        public IList<SkillDto> GetSkills(string userQuery)
        {
            string cacheKey = $"GetSkills{userQuery}";
            IList<SkillDto> rval = GetCachedValue<IList<SkillDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetSkills(userQuery);
                SetCachedValue<IList<SkillDto>>(cacheKey, rval);
            }
            return rval;
        }

        #endregion

        #region Public UnCached Methods

        public SubscriberDto Subscriber(Guid SubscriberGuid)
        {
            return Get<SubscriberDto>("profile/" + SubscriberGuid, true);
        }

        public PromoCodeDto PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseGuid, string subscriberGuid)
        {
            return Get<PromoCodeDto>("promocode/promocoderedemptionvalidation/" + promoCodeRedemptionGuid + "/" + courseGuid + "/" + subscriberGuid, true);
        }

        public PromoCodeDto PromoCodeValidation(string code, string courseVariantGuid, string subscriberGuid)
        {
            return Get<PromoCodeDto>("promocode/" + code + "/" + courseVariantGuid + "/" + subscriberGuid, true);
        }

        public VendorStudentLoginDto StudentLogin(int SubscriberId)
        {
            return Get<VendorStudentLoginDto>("enrollment/StudentLogin/" + SubscriberId.ToString(), true);
        }

        public CourseLoginDto CourseLogin(Guid SubscriberGuid, Guid EnrollmentGuid)
        {
            return Get<CourseLoginDto>($"course/StudentLoginUrl/{SubscriberGuid}/{EnrollmentGuid}", true);
        }


        public BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber)
        {
            return Post<BasicResponseDto>(Subscriber, "profile/update", true);
        }

        public BasicResponseDto SyncLinkedInAccount(Guid SubscriberGuid, string linkedInCode, string returnUrl)
        {
            return Get<BasicResponseDto>($"linkedin/SyncProfile/{SubscriberGuid}/{linkedInCode}?returnUrl={returnUrl}",true);
        }

        public Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto)
        {
            return Post<Guid>(enrollmentFlowDto, "enrollment/", true);
        }

        public Guid WriteToEnrollmentLog(EnrollmentLogDto enrollmentLogDto)
        {
            return Post<Guid>(enrollmentLogDto, "enrollment/EnrollmentLog", true);
        }

        public SubscriberDto CreateSubscriber(string SubscriberGuid, string SubscriberEmail)
        {
            return Post<SubscriberDto>("profile/CreateSubscriber/" + SubscriberGuid + "/" + Uri.EscapeDataString(SubscriberEmail), true);
        }

        public WozCourseProgressDto UpdateStudentCourseProgress(Guid SubscriberGuid, bool FutureSchedule)
        {

            return Put<WozCourseProgressDto>("woz/UpdateStudentCourseStatus/" + SubscriberGuid + "/" + FutureSchedule.ToString(), true);
        }

        public BraintreeResponseDto SubmitBraintreePayment(BraintreePaymentDto BraintreePaymentDto)
        {
            return Post<BraintreeResponseDto>(BraintreePaymentDto, "enrollment/ProcessBraintreePayment", true);
        }

        #endregion

        #region Cache Helper Functions

        private IList<TopicDto> _Topics()
        {
            return Get<IList<TopicDto>>("topic", false);
        }

        private TopicDto _TopicById(int TopicId)
        {
            return Get<TopicDto>($"topic/{TopicId}", false);
        }

        private TopicDto _TopicBySlug(string TopicSlug)
        {
            return Get<TopicDto>("topic/slug/" + TopicSlug, false);
        }

        public IList<CountryDto> _GetCountries()
        {
            return Get<IList<CountryDto>>("profile/GetCountries", false);
        }

        public IList<StateDto> _GetStatesByCountry(Guid? countryGuid)
        {
            return Get<IList<StateDto>>("profile/GetStatesByCountry/" + countryGuid?.ToString(), false);
        }

        private IList<CourseDto> _getCousesByTopicSlug(string TopicSlug)
        {
            return Get<IList<CourseDto>>("course/" + TopicSlug, false);
        }

        private CourseDto _Course(string CourseSlug)
        {
            CourseDto retVal = Get<CourseDto>("course/slug/" + CourseSlug, false);
            return retVal;
        }

        private CourseDto _CourseByGuid(Guid CourseGuid)
        {
            CourseDto retVal = Get<CourseDto>("course/guid/" + CourseGuid, false);
            return retVal;
        }
        public CourseVariantDto _GetCourseVariant(Guid courseVariantGuid)
        {
            return Get<CourseVariantDto>("course/GetCourseVariant/" + courseVariantGuid, false);
        }

        private WozTermsOfServiceDto _GetWozTermsOfService()
        {
            return Get<WozTermsOfServiceDto>("woz/TermsOfService/", false);
        }
  
        private WozCourseProgressDto _GetCurrentCourseProgress(Guid SubscriberGuid, Guid EnrollmentGuid)
        {
            return Get<WozCourseProgressDto>("woz/CourseStatus/" + SubscriberGuid + "/" + EnrollmentGuid, false);
        }

        private IList<SkillDto> _GetSkills(string userQuery)
        {
            return Get<IList<SkillDto>>("profile/GetSkills/" + userQuery, true);
        }

        public IList<SkillDto> GetSkillsBySubscriber(Guid subscriberGuid)
        {
            return Get<IList<SkillDto>>("profile/GetSkillsBySubscriber/" + subscriberGuid, true);
        }
        #endregion

        #region Private Helper Functions

        private bool SetCachedValue<T>(string CacheKey, T Value)
        {
            try
            {
                int CacheTTL = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
                string newValue = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                _cache.SetString(CacheKey, newValue, new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddHours(CacheTTL) });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        private T GetCachedValue <T>(string CacheKey)
        {
            try
            {
                string existingValue = _cache.GetString(CacheKey);
                if ( string.IsNullOrEmpty(existingValue) )
                    return(T)Convert.ChangeType(null, typeof(T));
                else
                {
                    T rval = JsonConvert.DeserializeObject<T>(existingValue);
                    return rval;
                }                
            }
            catch (Exception ex)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }

        #endregion

        #region ApiHelperMsal
        public T Put<T>(string ApiAction, bool Authorized = false)
        {
            Task<string> Response = _PutAsync(ApiAction, Authorized);
            try
            {
                T rval = JsonConvert.DeserializeObject<T>(Response.Result);
                return rval;
            }
            catch (Exception ex)
            {
                // TODO instrument with json string and requested type 
                var msg = ex.Message;
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }

        public T Get<T>(string ApiAction, bool Authorized = false)
        {
            Task<string> Response = _GetAsync(ApiAction, Authorized);
            try
            {
                T rval = JsonConvert.DeserializeObject<T>(Response.Result);
                return rval;
            }
            catch (Exception ex)
            {
                // TODO instrument with json string and requested type 
                var msg = ex.Message;
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }

        public T Post<T>(string ApiAction, bool Authorized = false, string Content = null)
        {
            Task<string> Response = _PostAsync(ApiAction, Authorized, Content);
            try
            {
                T rval = JsonConvert.DeserializeObject<T>(Response.Result);
                return rval;
            }
            catch (Exception ex)
            {
                // TODO instrument with json string and requested type 
                var msg = ex.Message;
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }

        public string GetAsString(string ApiAction, bool Authorized = false)
        {
            Task<string> Response = _GetAsync(ApiAction, Authorized);
            return Response.Result;
        }
 
        public T Post<T>(BaseDto Body, string ApiAction, bool Authorized = false)
        {
            string jsonToSend = "{}";
            jsonToSend = JsonConvert.SerializeObject(Body);
            Task<string> Response = _PostAsync(jsonToSend, ApiAction, Authorized);
            T rval = JsonConvert.DeserializeObject<T>(Response.Result);
            return rval;

        }

        #region Helpers Functions

        private async Task AddBearerTokenAsync(HttpRequestMessage request)
        {
            // Retrieve the token with the specified scopes
            var scope = AzureOptions.ApiScopes.Split(' ');
            string signedInUserID = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            TokenCache userTokenCache = new MSALSessionCache(signedInUserID, _contextAccessor.HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cca = new ConfidentialClientApplication(AzureOptions.ClientId, AzureOptions.Authority, AzureOptions.RedirectUri, new ClientCredential(AzureOptions.ClientSecret), userTokenCache, null);
            AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, cca.Users.FirstOrDefault(), AzureOptions.Authority, false);
            // Add Bearer Token for MSAL authenticatiopn
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        }

        private async Task<string> _PostAsync(string ApiAction, bool Authorized, string Content)
        {
            string responseString = "";
            try
            {                
                HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpPostClientName);
                string ApiUrl = _ApiBaseUri + ApiAction;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                if (!string.IsNullOrEmpty(Content))
                    request.Content = new StringContent(Content);

                // Add token to the Authorization header and make the request 
                if (Authorized)
                    await AddBearerTokenAsync(request);

                HttpResponseMessage response = await client.SendAsync(request);
                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responseString = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.Unauthorized:
                        responseString = $"Please sign in again. {response.ReasonPhrase}";
                        break;
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            return responseString;

        }

        private async Task<string> _PutAsync(string ApiAction, bool Authorized = false)
        {
            string responseString = "";
            try
            {
                HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpPutClientName);
                string ApiUrl = _ApiBaseUri + ApiAction;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, ApiUrl);

                // Add token to the Authorization header and make the request 
                if (Authorized)
                    await AddBearerTokenAsync(request);

                HttpResponseMessage response = await client.SendAsync(request);
                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responseString = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.Unauthorized:
                        responseString = $"Please sign in again. {response.ReasonPhrase}";
                        break;
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            return responseString;

        }

        private async Task<string> _GetAsync(string ApiAction, bool Authorized = false)
        {
            string responseString = "";
            try
            {
                HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpGetClientName);
                string ApiUrl = _ApiBaseUri + ApiAction;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);

                // Add token to the Authorization header and make the request 
                if (Authorized)
                    await AddBearerTokenAsync(request);

                HttpResponseMessage response = await client.SendAsync(request);
                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responseString = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.Unauthorized:
                        responseString = $"Please sign in again. {response.ReasonPhrase}";
                        break;
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            return responseString;

        }

        private async Task<string> _PostAsync(String JsonToSend, string ApiAction, bool Authorized = false)
        {
                string ApiUrl = _ApiBaseUri + ApiAction;
                HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpPostClientName);
                var request = PostRequest(ApiAction, JsonToSend);
                // Add token to the Authorization header and make the request 
                if (Authorized)
                    await AddBearerTokenAsync(request);
                var response = await client.SendAsync(request);
                string responseString = await response.Content.ReadAsStringAsync();
                return responseString;
        }

        private HttpRequestMessage PostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _ApiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }

        private static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }

        #endregion

        #endregion
    }
}
 


