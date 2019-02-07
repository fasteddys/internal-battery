using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Linq;
using UpDiddy.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
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
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;

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
        public ApiUpdiddy(IOptions<AzureAdB2COptions> azureAdB2COptions, IHttpContextAccessor contextAccessor, IConfiguration conifguration, IHttpClientFactory httpClientFactory, IDistributedCache cache)
        {

            AzureOptions = azureAdB2COptions.Value;
            _contextAccessor = contextAccessor;
            _configuration = conifguration;
            // Set the base URI for API calls 
            _ApiBaseUri = _configuration["Api:ApiUrl"];
            _HttpClientFactory = httpClientFactory;
            _cache = cache;

        }
        #endregion

        public async Task<T> GetAsync<T>(string endpoint)
        {
            HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpGetClientName);
            client.BaseAddress = new Uri(_ApiBaseUri);
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var scope = AzureOptions.ApiScopes.Split(' ');
                string signedInUserID = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                TokenCache userTokenCache = new MSALSessionCache(signedInUserID, _contextAccessor.HttpContext).GetMsalCacheInstance();
                ConfidentialClientApplication cca = new ConfidentialClientApplication(AzureOptions.ClientId, AzureOptions.Authority, AzureOptions.RedirectUri, new ClientCredential(AzureOptions.ClientSecret), userTokenCache, null);
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, cca.Users.FirstOrDefault(), AzureOptions.Authority, false);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            using (var response = await client.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

                throw new Exception("TEST");
            }
        }

        #region Public Cached Methods 
        public async Task<IList<TopicDto>> TopicsAsync()
        {
            string cacheKey = "Topics";
            IList<TopicDto> rval = GetCachedValue<IList<TopicDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _TopicsAsync();
                SetCachedValue<IList<TopicDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<TopicDto> TopicByIdAsync(int TopicId)
        {
            string cacheKey = $"TopicById{TopicId}";
            TopicDto rval = GetCachedValue<TopicDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _TopicByIdAsync(TopicId);
                SetCachedValue<TopicDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<TopicDto> TopicBySlugAsync(string TopicSlug)
        {
            string cacheKey = $"TopicBySlug{TopicSlug}";
            TopicDto rval = GetCachedValue<TopicDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _TopicBySlugAsync(TopicSlug);
                SetCachedValue<TopicDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CourseDto>> getCoursesByTopicSlugAsync(string TopicSlug)
        {
            string cacheKey = $"getCousesByTopicSlug{TopicSlug}";
            IList<CourseDto> rval = GetCachedValue<IList<CourseDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _getCoursesByTopicSlugAsync(TopicSlug);
                SetCachedValue<IList<CourseDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CourseDto>> CoursesAsync()
        {
            string cacheKey = $"getCourses";
            IList<CourseDto> rval = GetCachedValue<IList<CourseDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _CoursesAsync();
                SetCachedValue<IList<CourseDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<CourseDto> CourseAsync(string CourseSlug)
        {
            string cacheKey = $"Course{CourseSlug}";
            CourseDto rval = GetCachedValue<CourseDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _CourseAsync(CourseSlug);
                SetCachedValue<CourseDto>(cacheKey, rval);
            }
            return rval;

        }

        public async Task<IList<CountryDto>> GetCountriesAsync()
        {
            string cacheKey = $"GetCountries";
            IList<CountryDto> rval = GetCachedValue<IList<CountryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCountriesAsync();
                SetCachedValue<IList<CountryDto>>(cacheKey, rval);
            }
            return rval;
        }
        public async Task<IList<StateDto>> GetStatesByCountryAsync(Guid? countryGuid)
        {
            string cacheKey = $"GetStatesByCountry{countryGuid}";
            IList<StateDto> rval = GetCachedValue<IList<StateDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetStatesByCountryAsync(countryGuid);
                SetCachedValue<IList<StateDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<CourseVariantDto> GetCourseVariantAsync(Guid courseVariantGuid)
        {
            string cacheKey = $"GetCourseVariant{courseVariantGuid}";
            CourseVariantDto rval = GetCachedValue<CourseVariantDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCourseVariantAsync(courseVariantGuid);
                SetCachedValue<CourseVariantDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<SkillDto>> GetSkillsAsync(string userQuery)
        {
            string cacheKey = $"GetSkills{userQuery}";
           IList<SkillDto> rval = GetCachedValue<IList<SkillDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetSkillsAsync(userQuery);
                SetCachedValue<IList<SkillDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CompanyDto>> GetCompaniesAsync(string userQuery)
        {
            string cacheKey = $"GetCompanies{userQuery}";
            IList<CompanyDto> rval = GetCachedValue<IList<CompanyDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCompaniesAsync(userQuery);
                SetCachedValue<IList<CompanyDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<EducationalInstitutionDto>> GetEducationalInstitutionsAsync(string userQuery)
        {
            string cacheKey = $"GetEducationalInstitutions{userQuery}";
            IList<EducationalInstitutionDto> rval = GetCachedValue<IList<EducationalInstitutionDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationalInstitutionsAsync(userQuery);
                SetCachedValue<IList<EducationalInstitutionDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<EducationalDegreeDto>> GetEducationalDegreesAsync(string userQuery)
        {
            string cacheKey = $"GetEducationalDegrees{userQuery}";
            IList<EducationalDegreeDto> rval = GetCachedValue<IList<EducationalDegreeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationalDegreesAsync(userQuery);
                SetCachedValue<IList<EducationalDegreeDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CompensationTypeDto>> GetCompensationTypesAsync()
        {
            string cacheKey = $"GetCompensationTypes";
            IList<CompensationTypeDto> rval = GetCachedValue<IList<CompensationTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCompensationTypesAsync();
                SetCachedValue<IList<CompensationTypeDto>>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<IList<EducationalDegreeTypeDto>> GetEducationalDegreeTypesAsync()
        {
            string cacheKey = $"GetEducationDegreeTypes";
            IList<EducationalDegreeTypeDto> rval = GetCachedValue<IList<EducationalDegreeTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationalDegreeTypesAsync();
                SetCachedValue<IList<EducationalDegreeTypeDto>>(cacheKey, rval);
            }
            return rval;
        }

        public CourseDto GetCourseByCampaignGuid(Guid CampaignGuid)
        {
            string cacheKey = $"GetCourseByCampaignGuid{CampaignGuid}";
            CourseDto rval = GetCachedValue<CourseDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetCourseByCampaignGuid(CampaignGuid);
                SetCachedValue<CourseDto>(cacheKey, rval);
            }
            return rval;

        }




        #endregion

        #region Public UnCached Methods

        public async Task<PromoCodeDto> PromoCodeRedemptionValidationAsync(string promoCodeRedemptionGuid, string courseGuid)
        {
            return await GetAsync<PromoCodeDto>("promocode/redemption-validate/" + promoCodeRedemptionGuid + "/course-variant/" + courseGuid);
        }

        public async Task<PromoCodeDto> PromoCodeValidationAsync(string code, string courseVariantGuid)
        {
            return await GetAsync<PromoCodeDto>("promocode/validate/" + code + "/course-variant/" + courseVariantGuid);
        }

        public async Task<CourseLoginDto> CourseLoginAsync(Guid EnrollmentGuid)
        {
            return await GetAsync<CourseLoginDto>($"enrollment/{EnrollmentGuid}/student-login-url");
        }

        public BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber)
        {
            return Put<BasicResponseDto>(Subscriber, "subscriber", true);
        }

        public BasicResponseDto UpdateEntitySkills(EntitySkillDto entitySkillDto)
        {
            return Put<BasicResponseDto>(entitySkillDto, "skill/update", true);
        }

        public async Task<IList<SkillDto>> GetEntitySkillsAsync(string entityType, Guid entityGuid)
        {
            return await GetAsync<IList<SkillDto>>($"skill/get/{entityType}/{entityGuid}");
        }
        public BasicResponseDto UpdateOnboardingStatus()
        {
            return Put<BasicResponseDto>("subscriber/onboard", true);
        }

        public BasicResponseDto SyncLinkedInAccount(string linkedInCode, string returnUrl)
        {
            return Put<BasicResponseDto>($"linkedin/sync-profile/{linkedInCode}?returnUrl={returnUrl}", true);
        }

        public Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto)
        {
            return Post<Guid>(enrollmentFlowDto, "enrollment/", true);
        }

        public SubscriberDto CreateSubscriber()
        {
            return Post<SubscriberDto>("subscriber", true);
        }

        public WozCourseProgressDto UpdateStudentCourseProgress(bool FutureSchedule)
        {
            return Put<WozCourseProgressDto>("course/update-student-course-status/" + FutureSchedule.ToString(), true);
        }

        public BraintreeResponseDto SubmitBraintreePayment(BraintreePaymentDto BraintreePaymentDto)
        {
            return Post<BraintreeResponseDto>(BraintreePaymentDto, "enrollment/ProcessBraintreePayment", true);
        }

        public BasicResponseDto UploadResume(ResumeDto resumeDto)
        {
            return Post<BasicResponseDto>(resumeDto, "resume/upload", true);
        }

        #region Subscriber Work History
        public SubscriberWorkHistoryDto AddWorkHistory(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory)
        {
            return Post<SubscriberWorkHistoryDto>(workHistory, string.Format("subscriber/{0}/work-history", subscriberGuid.ToString()), true);
        }
        public SubscriberWorkHistoryDto UpdateWorkHistory(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory)
        {
            return Put<SubscriberWorkHistoryDto>(workHistory, string.Format("subscriber/{0}/work-history", subscriberGuid.ToString()), true);
        }
        
        public async Task<IList<SubscriberWorkHistoryDto>> GetWorkHistoryAsync(Guid subscriberGuid)
        {
            return await GetAsync<IList<SubscriberWorkHistoryDto>>(string.Format("subscriber/{0}/work-history", subscriberGuid.ToString()));
        }
        // Chris Put Delete in here and change path
        public SubscriberWorkHistoryDto DeleteWorkHistory(Guid subscriberGuid, Guid workHistoryGuid)
        {
            return Delete<SubscriberWorkHistoryDto>(string.Format("subscriber/{0}/work-history/{1}",subscriberGuid.ToString(), workHistoryGuid.ToString()) , true);
        }
        #endregion

        #region Subscriber Education History
        public SubscriberEducationHistoryDto AddEducationalHistory(Guid subscriberGuid, SubscriberEducationHistoryDto educationHistory)
        {
            return Post<SubscriberEducationHistoryDto>(educationHistory, string.Format("subscriber/{0}/education-history", subscriberGuid.ToString()), true);
        }


        public async Task<IList<SubscriberEducationHistoryDto>> GetEducationHistoryAsync(Guid subscriberGuid)
        {
            return await GetAsync<IList<SubscriberEducationHistoryDto>>(string.Format("subscriber/{0}/education-history", subscriberGuid.ToString()));
        }


        public SubscriberEducationHistoryDto UpdateEducationHistory(Guid subscriberGuid, SubscriberEducationHistoryDto educationHistory)
        {
            return Put<SubscriberEducationHistoryDto>(educationHistory, string.Format("subscriber/{0}/education-history", subscriberGuid.ToString()), true);
        }

        // Chris Put Delete in here and change path
        public SubscriberEducationHistoryDto DeleteEducationHistory(Guid subscriberGuid, Guid educationHistory)
        {
            return Delete<SubscriberEducationHistoryDto>(string.Format("subscriber/{0}/education-history/{1}", subscriberGuid.ToString(), educationHistory.ToString()), true);
        }
        #endregion

        public BasicResponseDto UpdateSubscriberContact(Guid contactGuid, SignUpDto signUpDto)
        {
            // encrypt password before sending to API
            signUpDto.password = Crypto.Encrypt(_configuration["Crypto:Key"], signUpDto.password);

            return Put<BasicResponseDto>(signUpDto, string.Format("subscriber/contact/{0}", contactGuid.ToString()), false);
        }

        public async Task<SubscriberADGroupsDto> MyGroupsAsync()
        {
            return await GetAsync<SubscriberADGroupsDto>("subscriber/me/group");
        }

        public async Task<HttpResponseMessage> DownloadFileAsync(Guid subscriberGuid, int fileId)
        {
            HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpGetClientName);
            string ApiUrl = _ApiBaseUri + String.Format("subscriber/{0}/file/{1}", subscriberGuid, fileId);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);

            // Add token to the Authorization header and make the request
            await AddBearerTokenAsync(request);

            HttpResponseMessage response = await client.SendAsync(request);
            return response;
        }
        #endregion

        #region Cache Helper Functions

            private async Task<IList<TopicDto>> _TopicsAsync()
        {
            return await GetAsync<IList<TopicDto>>("topic");
        }

        private async Task<TopicDto> _TopicByIdAsync(int TopicId)
        {
            return await GetAsync<TopicDto>($"topic/{TopicId}");
        }

        private async Task<TopicDto> _TopicBySlugAsync(string TopicSlug)
        {
            return await GetAsync<TopicDto>("topic/slug/" + TopicSlug);
        }

        public async Task<IList<CountryDto>> _GetCountriesAsync()
        {
            return await GetAsync<IList<CountryDto>>("country");
        }

        public async Task<IList<StateDto>> _GetStatesByCountryAsync(Guid? countryGuid)
        {
            if (!countryGuid.HasValue)
                return await GetStatesAsync();

            return await GetAsync<IList<StateDto>>("country/" + countryGuid?.ToString() + "/state");
        }

        public async Task<IList<StateDto>> GetStatesAsync()
        {
            return await GetAsync<IList<StateDto>>("state/");
        }

        private async Task<IList<CourseDto>> _getCoursesByTopicSlugAsync(string TopicSlug)
        {
            return await GetAsync<IList<CourseDto>>("course/topic/" + TopicSlug);
        }
        private async Task<IList<CourseDto>> _CoursesAsync()
        {
            return await GetAsync<IList<CourseDto>>("course/");
        }

        private async Task<CourseDto> _CourseAsync(string CourseSlug)
        {
            CourseDto retVal = await GetAsync<CourseDto>("course/slug/" + CourseSlug);
            return retVal;
        }

        public async Task<CourseVariantDto> _GetCourseVariantAsync(Guid courseVariantGuid)
        {
            return await GetAsync<CourseVariantDto>("course/course-variant/" + courseVariantGuid);
        }

        private async Task<IList<SkillDto>> _GetSkillsAsync(string userQuery)
        {
            return await GetAsync<IList<SkillDto>>("skill/" + userQuery);
        }


        private async Task<IList<CompanyDto>> _GetCompaniesAsync(string userQuery)
        {
            return await GetAsync<IList<CompanyDto>>("company/" + userQuery);
        }

        private async Task<IList<EducationalInstitutionDto>> _GetEducationalInstitutionsAsync(string userQuery)
        {
            return await GetAsync<IList<EducationalInstitutionDto>>("educational-institution/" + userQuery);
        }

        private async Task<IList<EducationalDegreeDto>> _GetEducationalDegreesAsync(string userQuery)
        {
            return await GetAsync<IList<EducationalDegreeDto>>("educational-degree/" + userQuery);
        }


        private async Task<IList<CompensationTypeDto>> _GetCompensationTypesAsync()
        {
            return await GetAsync<IList<CompensationTypeDto>>("compensation-types");
        }



        private async Task<IList<EducationalDegreeTypeDto>> _GetEducationalDegreeTypesAsync()
        { 
            return await GetAsync<IList<EducationalDegreeTypeDto>>("educational-degree-types");
        }


        public async Task<IList<SkillDto>> GetSkillsBySubscriberAsync(Guid subscriberGuid)
        {
            return await GetAsync<IList<SkillDto>>("subscriber/" + subscriberGuid + "/skill");
        }

        public CourseDto _GetCourseByCampaignGuid(Guid CampaignGuid)
        {
            return Get<CourseDto>("course/campaign/" + CampaignGuid, false);
        }
        #endregion

        #region Private Helper Functions

        private bool SetCachedValue<T>(string CacheKey, T Value)
        {
            try
            {
                int CacheTTL = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
                string newValue = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                _cache.SetString(CacheKey, newValue, new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(CacheTTL) });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private T GetCachedValue<T>(string CacheKey)
        {
            try
            {
                string existingValue = _cache.GetString(CacheKey);
                if (string.IsNullOrEmpty(existingValue))
                    return (T)Convert.ChangeType(null, typeof(T));
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

        public T Delete<T>(string ApiAction, bool Authorized = false)
        {
            Task<string> Response = _DeleteAsync(ApiAction, Authorized);
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

        #region TalentPortal

        public async Task<SubscriberDto> SubscriberAsync(Guid subscriberGuid, bool hardRefresh)
        {
            string cacheKey = $"Subscriber{subscriberGuid}";
            SubscriberDto rval = null;
            if(!hardRefresh)
                rval = GetCachedValue<SubscriberDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _SubscriberAsync(subscriberGuid);
                if (rval == null)
                    rval = CreateSubscriber();
                SetCachedValue<SubscriberDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<SubscriberDto>> SubscriberSearchAsync(string searchQuery)
        {
            string cacheKey = $"SubscriberSearch{searchQuery}";
            IList<SubscriberDto> rval = GetCachedValue<IList<SubscriberDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _SubscriberSearchAsync(searchQuery);
                SetCachedValue<IList<SubscriberDto>>(cacheKey, rval);
            }
            return rval;
        }

        private async Task<IList<SubscriberDto>> _SubscriberSearchAsync(string searchQuery)
        {
            return await GetAsync<IList<SubscriberDto>>($"subscriber/search/{searchQuery}");
        }

        private async Task<SubscriberDto> _SubscriberAsync(Guid subscriberGuid)
        {
            return await GetAsync<SubscriberDto>($"subscriber/{subscriberGuid}");
        }

        #endregion

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

        public T Put<T>(BaseDto Body, string ApiAction, bool Authorized = false)
        {
            string json = JsonConvert.SerializeObject(Body);
            Task<string> Response = _PutAsync(json, ApiAction, Authorized);
            T response = JsonConvert.DeserializeObject<T>(Response.Result);
            return response;
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

        private async Task<string> _DeleteAsync(string ApiAction, bool Authorized = false)
        {
            string responseString = "";
            try
            {
                HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpDeleteClientName);
                string ApiUrl = _ApiBaseUri + ApiAction;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, ApiUrl);

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

        private async Task<string> _PutAsync(String json, string ApiAction, bool Authorized = false)
        {
            string ApiUrl = _ApiBaseUri + ApiAction;
            HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpPutClientName);
            var request = Request(HttpMethod.Put, ApiAction, json);

            if (Authorized)
                await AddBearerTokenAsync(request);

            var response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;

        }

        private HttpRequestMessage Request(HttpMethod method, string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, _ApiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage PostRequest(string ApiAction, string Content)
        {
            return Request(HttpMethod.Post, ApiAction, Content);
        }
        #endregion

        #endregion
    }
}



