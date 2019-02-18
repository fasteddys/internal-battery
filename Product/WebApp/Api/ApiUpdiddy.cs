using System;
using System.IO;
using System.Net;
using System.Text;
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

        public IList<CourseDto> getCoursesByTopicSlug(string TopicSlug)
        {
            string cacheKey = $"getCousesByTopicSlug{TopicSlug}";
            IList<CourseDto> rval = GetCachedValue<IList<CourseDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _getCoursesByTopicSlug(TopicSlug);
                SetCachedValue<IList<CourseDto>>(cacheKey, rval);
            }
            return rval;
        }

        public IList<CourseDto> Courses()
        {
            string cacheKey = $"getCourses";
            IList<CourseDto> rval = GetCachedValue<IList<CourseDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _Courses();
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

        public IList<CompanyDto> GetCompanies(string userQuery)
        {
            string cacheKey = $"GetCompanies{userQuery}";
            IList<CompanyDto> rval = GetCachedValue<IList<CompanyDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetCompanies(userQuery);
                SetCachedValue<IList<CompanyDto>>(cacheKey, rval);
            }
            return rval;
        }

        public IList<EducationalInstitutionDto> GetEducationalInstitutions(string userQuery)
        {
            string cacheKey = $"GetEducationalInstitutions{userQuery}";
            IList<EducationalInstitutionDto> rval = GetCachedValue<IList<EducationalInstitutionDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetEducationalInstitutions(userQuery);
                SetCachedValue<IList<EducationalInstitutionDto>>(cacheKey, rval);
            }
            return rval;
        }

        public IList<EducationalDegreeDto> GetEducationalDegrees(string userQuery)
        {
            string cacheKey = $"GetEducationalDegrees{userQuery}";
            IList<EducationalDegreeDto> rval = GetCachedValue<IList<EducationalDegreeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetEducationalDegrees(userQuery);
                SetCachedValue<IList<EducationalDegreeDto>>(cacheKey, rval);
            }
            return rval;
        }

        public IList<CompensationTypeDto> GetCompensationTypes()
        {
            string cacheKey = $"GetCompensationTypes";
            IList<CompensationTypeDto> rval = GetCachedValue<IList<CompensationTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetCompensationTypes();
                SetCachedValue<IList<CompensationTypeDto>>(cacheKey, rval);
            }
            return rval;
        }


        public IList<EducationalDegreeTypeDto> GetEducationalDegreeTypes()
        {
            string cacheKey = $"GetEducationDegreeTypes";
            IList<EducationalDegreeTypeDto> rval = GetCachedValue<IList<EducationalDegreeTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetEducationalDegreeTypes();
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

        public ContactDto Contact(Guid contactGuid)
        {
            string cacheKey = $"Contact{contactGuid}";
            ContactDto rval = GetCachedValue<ContactDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _Contact(contactGuid);
                SetCachedValue<ContactDto>(cacheKey, rval);
            }
            return rval;
        }




        #endregion

        #region Public UnCached Methods

        public PromoCodeDto PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseGuid)
        {
            return Get<PromoCodeDto>("promocode/redemption-validate/" + promoCodeRedemptionGuid + "/course-variant/" + courseGuid, true);
        }

        public PromoCodeDto PromoCodeValidation(string code, string courseVariantGuid)
        {
            return Get<PromoCodeDto>("promocode/validate/" + code + "/course-variant/" + courseVariantGuid, true);
        }

        public CourseLoginDto CourseLogin(Guid EnrollmentGuid)
        {
            return Get<CourseLoginDto>($"enrollment/{EnrollmentGuid}/student-login-url", true);
        }

        public BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber)
        {
            return Put<BasicResponseDto>(Subscriber, "subscriber", true);
        }

        public BasicResponseDto UpdateEntitySkills(EntitySkillDto entitySkillDto)
        {
            return Put<BasicResponseDto>(entitySkillDto, "skill/update", true);
        }

        public IList<SkillDto> GetEntitySkills(string entityType, Guid entityGuid)
        {
            return Get<IList<SkillDto>>($"skill/get/{entityType}/{entityGuid}", true);
        }
        public BasicResponseDto UpdateOnboardingStatus()
        {
            return Put<BasicResponseDto>("subscriber/onboard", true);
        }

        public BasicResponseDto SyncLinkedInAccount(string linkedInCode, string returnUrl)
        {
            return Put<BasicResponseDto>($"linkedin/sync-profile/{linkedInCode}?returnUrl={returnUrl}", true);
        }

        public LinkedInProfileDto GetLinkedInProfile()
        {
            return Get<LinkedInProfileDto>("linkedin", true);
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
        
        public IList<SubscriberWorkHistoryDto> GetWorkHistory(Guid subscriberGuid)
        {
            return Get<IList<SubscriberWorkHistoryDto>>(string.Format("subscriber/{0}/work-history", subscriberGuid.ToString()), true);
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


        public IList<SubscriberEducationHistoryDto> GetEducationHistory(Guid subscriberGuid)
        {
            return Get<IList<SubscriberEducationHistoryDto>>(string.Format("subscriber/{0}/education-history", subscriberGuid.ToString()), true);
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

        public SubscriberADGroupsDto MyGroups()
        {
            return Get<SubscriberADGroupsDto>("subscriber/me/group", true);
        }

        public async Task<HttpResponseMessage> DownloadFileAsync(Guid subscriberGuid, Guid fileGuid)
        {
            HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpGetClientName);
            string ApiUrl = _ApiBaseUri + String.Format("subscriber/{0}/file/{1}", subscriberGuid, fileGuid.ToString());
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);

            // Add token to the Authorization header and make the request
            await AddBearerTokenAsync(request);

            HttpResponseMessage response = await client.SendAsync(request);
            return response;
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
            return Get<IList<CountryDto>>("country", false);
        }

        public IList<StateDto> _GetStatesByCountry(Guid? countryGuid)
        {
            if (!countryGuid.HasValue)
                return GetStates();

            return Get<IList<StateDto>>("country/" + countryGuid?.ToString() + "/state", false);
        }

        public IList<StateDto> GetStates()
        {
            return Get<IList<StateDto>>("state/", false);
        }

        private IList<CourseDto> _getCoursesByTopicSlug(string TopicSlug)
        {
            return Get<IList<CourseDto>>("course/topic/" + TopicSlug, false);
        }
        private IList<CourseDto> _Courses()
        {
            return Get<IList<CourseDto>>("course/", false);
        }

        private CourseDto _Course(string CourseSlug)
        {
            CourseDto retVal = Get<CourseDto>("course/slug/" + CourseSlug, false);
            return retVal;
        }

        public CourseVariantDto _GetCourseVariant(Guid courseVariantGuid)
        {
            return Get<CourseVariantDto>("course/course-variant/" + courseVariantGuid, false);
        }

        private IList<SkillDto> _GetSkills(string userQuery)
        {
            return Get<IList<SkillDto>>("skill/" + userQuery, true);
        }


        private IList<CompanyDto> _GetCompanies(string userQuery)
        {
            return Get<IList<CompanyDto>>("company/" + userQuery, true);
        }

        private IList<EducationalInstitutionDto> _GetEducationalInstitutions(string userQuery)
        {
            return Get<IList<EducationalInstitutionDto>>("educational-institution/" + userQuery, true);
        }

        private IList<EducationalDegreeDto> _GetEducationalDegrees(string userQuery)
        {
            return Get<IList<EducationalDegreeDto>>("educational-degree/" + userQuery, true);
        }


        private IList<CompensationTypeDto> _GetCompensationTypes()
        {
            return Get<IList<CompensationTypeDto>>("compensation-types" , true);
        }



        private IList<EducationalDegreeTypeDto> _GetEducationalDegreeTypes()
        { 
            return Get<IList<EducationalDegreeTypeDto>>("educational-degree-types", true);
        }


        public IList<SkillDto> GetSkillsBySubscriber(Guid subscriberGuid)
        {
            return Get<IList<SkillDto>>("subscriber/" + subscriberGuid + "/skill", true);
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

        public SubscriberDto Subscriber(Guid subscriberGuid, bool hardRefresh)
        {
            string cacheKey = $"Subscriber{subscriberGuid}";
            SubscriberDto rval = null;
            if(!hardRefresh)
                rval = GetCachedValue<SubscriberDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _Subscriber(subscriberGuid);
                if (rval == null)
                    rval = CreateSubscriber();
                SetCachedValue<SubscriberDto>(cacheKey, rval);
            }
            return rval;
        }

        public IList<SubscriberDto> SubscriberSearch(string searchQuery)
        {
            string cacheKey = $"SubscriberSearch{searchQuery}";
            IList<SubscriberDto> rval = GetCachedValue<IList<SubscriberDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _SubscriberSearch(searchQuery);
                SetCachedValue<IList<SubscriberDto>>(cacheKey, rval);
            }
            return rval;
        }

        private IList<SubscriberDto> _SubscriberSearch(string searchQuery)
        {
            return Get<IList<SubscriberDto>>($"subscriber/search/{searchQuery}", true);
        }

        private SubscriberDto _Subscriber(Guid subscriberGuid)
        {
            return Get<SubscriberDto>($"subscriber/{subscriberGuid}", true);
        }

        private ContactDto _Contact(Guid contactGuid)
        {
            return Get<ContactDto>($"contact/{contactGuid}", false);
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



