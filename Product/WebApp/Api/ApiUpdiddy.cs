using System;
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
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using UpDiddyLib.Dto.User;

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
        public IMemoryCache _memoryCache { get; set; }
        public HttpContext _currentContext { get; set; }
        private readonly ILogger _syslog;

        #region Constructor
        public ApiUpdiddy(IOptions<AzureAdB2COptions> azureAdB2COptions, IHttpContextAccessor contextAccessor, IConfiguration conifguration, IHttpClientFactory httpClientFactory, IDistributedCache cache, ILogger<ApiUpdiddy> sysLog, IMemoryCache memoryCache)
        {
            _syslog = sysLog;
            AzureOptions = azureAdB2COptions.Value;
            _contextAccessor = contextAccessor;
            _configuration = conifguration;
            // Set the base URI for API calls 
            _ApiBaseUri = _configuration["Api:ApiUrl"];
            _HttpClientFactory = httpClientFactory;
            _cache = cache;
            _currentContext = contextAccessor.HttpContext;
            _memoryCache = memoryCache;
        }
        #endregion

        #region Request Methods
        private async Task<HttpResponseMessage> RequestAsync(string clientName, HttpMethod method, string endpoint, object body = null, bool isAddUserManagementAuthorizationHeader = false)
        {
            HttpClient client = _HttpClientFactory.CreateClient(clientName);
            client.BaseAddress = new Uri(_ApiBaseUri);

            client = await AddBearerTokenAsync(client);

            if (isAddUserManagementAuthorizationHeader)
                client = AddUserManagementAuthorizationHeader(client);

            HttpRequestMessage request = new HttpRequestMessage(method, endpoint);

            if (body != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(body));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return await client.SendAsync(request);
        }

        private async Task<T> RequestAsync<T>(string clientName, HttpMethod method, string endpoint, object body = null, bool isAddUserManagementAuthorizationHeader = false)
        {
            using (var response = await RequestAsync(clientName, method, endpoint, body, isAddUserManagementAuthorizationHeader))
            {
                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

                throw new ApiException(response, JsonConvert.DeserializeObject<BasicResponseDto>(await response.Content.ReadAsStringAsync()));
            }
        }

        public async Task<T> GetAsync<T>(string endpoint, bool isAddUserManagementAuthorizationHeader = false)
        {
            return await RequestAsync<T>(Constants.HttpGetClientName, HttpMethod.Get, endpoint, null, isAddUserManagementAuthorizationHeader);
        }

        public async Task<T> PostAsync<T>(string endpoint, object body = null, bool isAddUserManagementAuthorizationHeader = false)
        {
            return await RequestAsync<T>(Constants.HttpPostClientName, HttpMethod.Post, endpoint, body, isAddUserManagementAuthorizationHeader);
        }

        public async Task<T> PutAsync<T>(string endpoint, object body = null, bool isAddUserManagementAuthorizationHeader = false)
        {
            return await RequestAsync<T>(Constants.HttpPutClientName, HttpMethod.Put, endpoint, body, isAddUserManagementAuthorizationHeader);
        }

        public async Task<T> DeleteAsync<T>(string endpoint, bool isAddUserManagementAuthorizationHeader = false)
        {
            return await RequestAsync<T>(Constants.HttpDeleteClientName, HttpMethod.Delete, endpoint, null, isAddUserManagementAuthorizationHeader);
        }

        private HttpClient AddUserManagementAuthorizationHeader(HttpClient client)
        {
            var clientSecret = _configuration["Auth0:ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ApplicationException("User management authorization header could not be found.");

            string encryptedClientSecret = Crypto.Encrypt(_configuration["Crypto:Key"], _configuration["Auth0:ClientSecret"]);

            client.DefaultRequestHeaders.Add("UserManagement", encryptedClientSecret);

            return client;
        }

        private async Task<HttpClient> AddBearerTokenAsync(HttpClient client)
        {
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string accessToken = _contextAccessor.HttpContext.User.FindFirst("access_token")?.Value;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken != null ? accessToken : string.Empty);
            }

            return client;
        }
        #endregion

        #region Public Cached Methods 
        public async Task<IList<TopicDto>> TopicsAsync()
        {
            string cacheKey = "Topics";
            IList<TopicDto> rval = GetCachedValueAsync<IList<TopicDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _TopicsAsync();
                SetCachedValueAsync<IList<TopicDto>>(cacheKey, rval);
            }
            return rval;

        }

        public async Task<TopicDto> TopicByIdAsync(int TopicId)
        {
            string cacheKey = $"TopicById{TopicId}";
            TopicDto rval = GetCachedValueAsync<TopicDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _TopicByIdAsync(TopicId);
                SetCachedValueAsync<TopicDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<TopicDto> TopicBySlugAsync(string TopicSlug)
        {
            string cacheKey = $"TopicBySlug{TopicSlug}";
            TopicDto rval = GetCachedValueAsync<TopicDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _TopicBySlugAsync(TopicSlug);
                SetCachedValueAsync<TopicDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CourseDto>> getCoursesByTopicSlugAsync(string TopicSlug)
        {
            string cacheKey = $"getCousesByTopicSlug{TopicSlug}";
            IList<CourseDto> rval = GetCachedValueAsync<IList<CourseDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _getCoursesByTopicSlugAsync(TopicSlug);
                SetCachedValueAsync<IList<CourseDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CourseDto>> CoursesAsync()
        {
            string cacheKey = $"getCourses";
            IList<CourseDto> rval = GetCachedValueAsync<IList<CourseDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _CoursesAsync();
                SetCachedValueAsync<IList<CourseDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<CourseDto> CourseAsync(string CourseSlug)
        {
            string cacheKey = $"Course{CourseSlug}";
            CourseDto rval = GetCachedValueAsync<CourseDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _CourseAsync(CourseSlug);
                SetCachedValueAsync<CourseDto>(cacheKey, rval);
            }
            return rval;

        }






        public async Task<IList<CountryDto>> GetCountriesAsync()
        {
            string cacheKey = $"GetCountries";
            IList<CountryDto> rval = GetCachedValueAsync<IList<CountryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCountriesAsync();
                SetCachedValueAsync<IList<CountryDto>>(cacheKey, rval);
            }
            return rval;
        }
        public async Task<IList<StateDto>> GetStatesByCountryAsync(Guid? countryGuid)
        {
            string cacheKey = $"GetStatesByCountry{countryGuid}";
            IList<StateDto> rval = GetCachedValueAsync<IList<StateDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetStatesByCountryAsync(countryGuid);
                SetCachedValueAsync<IList<StateDto>>(cacheKey, rval);
            }
            return rval;
        }



        public async Task<IList<ExperienceLevelDto>> GetExperienceLevelAsync()
        {
            string cacheKey = "GetExperienceLevelAsync";
            IList<ExperienceLevelDto> rval = GetCachedValueAsync<IList<ExperienceLevelDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetExperienceLevelAsync();
                SetCachedValueAsync<IList<ExperienceLevelDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<SecurityClearanceDto>> GetSecurityClearanceAsync()
        {
            string cacheKey = "GetSecurityClearanceAsync";
            IList<SecurityClearanceDto> rval = GetCachedValueAsync<IList<SecurityClearanceDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetSecurityClearanceAsync();
                SetCachedValueAsync<IList<SecurityClearanceDto>>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<IList<RecruiterCompanyDto>> GetRecruiterCompaniesAsync(Guid subscriberGuid)
        {
            string cacheKey = $"GetRecruiterCompaniesAsync{subscriberGuid}";
            IList<RecruiterCompanyDto> rval = GetCachedValueAsync<IList<RecruiterCompanyDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetRecruiterCompanyAsync(subscriberGuid);
                SetCachedValueAsync<IList<RecruiterCompanyDto>>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<IList<EmploymentTypeDto>> GetEmploymentTypeAsync()
        {
            string cacheKey = "GetEmploymentTypeAsync";
            IList<EmploymentTypeDto> rval = GetCachedValueAsync<IList<EmploymentTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEmploymentTypeAsync();
                SetCachedValueAsync<IList<EmploymentTypeDto>>(cacheKey, rval);
            }
            return rval;
        }



        public async Task<IList<CompensationTypeDto>> GetCompensationTypeAsync()
        {
            string cacheKey = "GetCompensationTypeAsync";
            IList<CompensationTypeDto> rval = GetCachedValueAsync<IList<CompensationTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCompensationTypeAsync();
                SetCachedValueAsync<IList<CompensationTypeDto>>(cacheKey, rval);
            }
            return rval;
        }




        public async Task<IList<EducationLevelDto>> GetEducationLevelAsync()
        {
            string cacheKey = "GetEducationLevelAsync";
            IList<EducationLevelDto> rval = GetCachedValueAsync<IList<EducationLevelDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationLevelAsync();
                SetCachedValueAsync<IList<EducationLevelDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<List<JobPostingDto>> GetAllJobsAsync()
        {
            string cacheKey = "GetAllJobsAsync";
            List<JobPostingDto> rval = GetCachedValueAsync<List<JobPostingDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetAllJobsAsync();
                SetCachedValueAsync<List<JobPostingDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<JobCategoryDto>> GetJobCategoryAsync()
        {
            string cacheKey = "GetJobCategoryAsync";
            IList<JobCategoryDto> rval = GetCachedValueAsync<IList<JobCategoryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetJobCategoryAsync();
                SetCachedValueAsync<IList<JobCategoryDto>>(cacheKey, rval);
            }
            return rval;
        }



        public async Task<IList<IndustryDto>> GetIndustryAsync()
        {
            string cacheKey = "GetIndustryAsync";
            IList<IndustryDto> rval = GetCachedValueAsync<IList<IndustryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetIndustryAsync();
                SetCachedValueAsync<IList<IndustryDto>>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<CourseVariantDto> GetCourseVariantAsync(Guid courseVariantGuid)
        {
            string cacheKey = $"GetCourseVariant{courseVariantGuid}";
            CourseVariantDto rval = GetCachedValueAsync<CourseVariantDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCourseVariantAsync(courseVariantGuid);
                SetCachedValueAsync<CourseVariantDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<SkillDto>> GetSkillsAsync(string userQuery)
        {
            string cacheKey = $"GetSkills{userQuery}";
            IList<SkillDto> rval = GetCachedValueAsync<IList<SkillDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetSkillsAsync(userQuery);
                SetCachedValueAsync<IList<SkillDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CompanyDto>> GetAllCompaniesAsync()
        {
            string cacheKey = $"GetAllCompanies";
            IList<CompanyDto> rval = GetCachedValueAsync<IList<CompanyDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetAllCompaniesAsync();
                SetCachedValueAsync<IList<CompanyDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CompanyDto>> GetCompaniesAsync(string userQuery)
        {
            string cacheKey = $"GetCompanies{userQuery}";
            IList<CompanyDto> rval = GetCachedValueAsync<IList<CompanyDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCompaniesAsync(userQuery);
                SetCachedValueAsync<IList<CompanyDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<EducationalInstitutionDto>> GetEducationalInstitutionsAsync(string userQuery)
        {
            string cacheKey = $"GetEducationalInstitutions{userQuery}";
            IList<EducationalInstitutionDto> rval = GetCachedValueAsync<IList<EducationalInstitutionDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationalInstitutionsAsync(userQuery);
                SetCachedValueAsync<IList<EducationalInstitutionDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<EducationalDegreeDto>> GetEducationalDegreesAsync(string userQuery)
        {
            string cacheKey = $"GetEducationalDegrees{userQuery}";
            IList<EducationalDegreeDto> rval = GetCachedValueAsync<IList<EducationalDegreeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationalDegreesAsync(userQuery);
                SetCachedValueAsync<IList<EducationalDegreeDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<CompensationTypeDto>> GetCompensationTypesAsync()
        {
            string cacheKey = $"GetCompensationTypes";
            IList<CompensationTypeDto> rval = GetCachedValueAsync<IList<CompensationTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCompensationTypesAsync();
                SetCachedValueAsync<IList<CompensationTypeDto>>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<IList<EducationalDegreeTypeDto>> GetEducationalDegreeTypesAsync()
        {
            string cacheKey = $"GetEducationDegreeTypes";
            IList<EducationalDegreeTypeDto> rval = GetCachedValueAsync<IList<EducationalDegreeTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationalDegreeTypesAsync();
                SetCachedValueAsync<IList<EducationalDegreeTypeDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<CourseDto> GetCourseByCampaignGuidAsync(Guid CampaignGuid)
        {
            string cacheKey = $"GetCourseByCampaignGuid{CampaignGuid}";
            CourseDto rval = GetCachedValueAsync<CourseDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCourseByCampaignGuid(CampaignGuid);
                SetCachedValueAsync<CourseDto>(cacheKey, rval);
            }
            return rval;

        }

        public async Task<ContactDto> ContactAsync(Guid partnerContactGuid)
        {
            string cacheKey = $"Contact{partnerContactGuid}";
            ContactDto rval = GetCachedValueAsync<ContactDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _Contact(partnerContactGuid);
                SetCachedValueAsync<ContactDto>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<BasicResponseDto> SubmitServiceOfferingPayment(ServiceOfferingTransactionDto serviceOfferingTransactionDto)
        {
            return await PostAsync<BasicResponseDto>("serviceOfferingOrder", serviceOfferingTransactionDto);
        }

        public async Task<ServiceOfferingOrderDto> GetSubscriberOrder(Guid OrderGuid)
        {
            return await GetAsync<ServiceOfferingOrderDto>("serviceOfferingOrder/subscriber-order/" + OrderGuid);
        }

        public async Task<IList<OfferDto>> GetOffersAsync()
        {
            string cacheKey = $"Offers";
            IList<OfferDto> rval = GetCachedValueAsync<IList<OfferDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetOffersAsync();
                SetCachedValueAsync<IList<OfferDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<PagingDto<UpDiddyLib.Dto.User.JobDto>> GetUserJobsOfInterest(int? page)
        {
            string endpoint = "subscriber/me/jobs";
            if (page.HasValue)
                endpoint = QueryHelpers.AddQueryString(endpoint, "page", page.Value.ToString());
            return await GetAsync<PagingDto<UpDiddyLib.Dto.User.JobDto>>(endpoint);
        }

        public async Task<PagingDto<JobPostingAlertDto>> GetUserJobAlerts(int? page, int? timeZoneOffset)
        {
            string endpoint = "subscriber/me/job-alerts";
            if (page.HasValue)
                endpoint = QueryHelpers.AddQueryString(endpoint, "page", page.Value.ToString());
            if (timeZoneOffset.HasValue)
                endpoint = QueryHelpers.AddQueryString(endpoint, "timeZoneOffset", timeZoneOffset.Value.ToString());
            return await GetAsync<PagingDto<JobPostingAlertDto>>(endpoint);
        }

        public async Task<JobPostingDto> GetJobAsync(Guid JobPostingGuid, GoogleCloudEventsTrackingDto dto = null)
        {
            string cacheKey = $"job-{JobPostingGuid}";
            JobPostingDto rval = GetCachedValueAsync<JobPostingDto>(cacheKey);
            if (rval == null)
            {
                rval = await _GetJobAsync(JobPostingGuid);
                SetCachedValueAsync<JobPostingDto>(cacheKey, rval);
            }

            #region analytics
            GoogleCloudEventsTrackingDto eventDto = null;
            if (dto != null && dto.RequestId != null)
                eventDto = await RecordClientEventAsync(JobPostingGuid, dto);

            rval.RequestId = eventDto?.RequestId;
            rval.ClientEventId = eventDto?.ClientEventId;
            #endregion

            return rval;
        }

        public async Task<BasicResponseDto> GetActiveJobCountAsync()
        {
            string cacheKey = "job-activeJobCount";
            BasicResponseDto rval = GetCachedValueAsync<BasicResponseDto>(cacheKey);
            if (rval == null)
            {
                rval = await _GetActiveJobCountAsync();
                SetCachedValueAsync<BasicResponseDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<JobSearchResultDto> GetJobsByLocation(string searchQueryParameterString)
        {

            var searchFilter = $"all/all/all/all/all/all/0{(string.IsNullOrWhiteSpace(searchQueryParameterString) ? "?" : searchQueryParameterString + "&")}page-size=100";
            string cacheKey = $"job-{searchFilter}";
            JobSearchResultDto rval = GetCachedValueAsync<JobSearchResultDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {

                rval = await _GetJobsByLocation(searchFilter);
                SetCachedValueAsync<JobSearchResultDto>(cacheKey, rval);
            }

            return rval;
        }

        public async Task<JobSearchResultDto> GetJobsUsingRoute(
            string Country = null,
            string State = null,
            string City = null,
            string Industry = null,
            string Category = null,
            int page = 0)
        {
            Country = Country ?? "all";
            State = State ?? "all";
            City = City ?? "all";
            Industry = Industry ?? "all";
            Category = Category ?? "all";

            var searchFilter = $"{Country}/{State}/{City}/{Industry}/{Category}/all/{page}";
            string cacheKey = string.Format("job-{0}/{1}/{2}/{3}/{4}/{5}", Country, State, City, Industry, Category, page);
            JobSearchResultDto rval = GetCachedValueAsync<JobSearchResultDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {

                rval = await _GetJobsByLocation(searchFilter);
                SetCachedValueAsync<JobSearchResultDto>(cacheKey, rval);
            }

            return rval;
        }

        #endregion

        #region Public UnCached Methods

        public async Task<BasicResponseDto> ExistingUserSignup(CreateUserDto createUserDto)
        {
            return await PostAsync<BasicResponseDto>("subscriber/existing-user-sign-up", createUserDto);
        }

        public async Task<BasicResponseDto> UpdateLastSignIn(Guid subscriberGuid)
        {
            return await PutAsync<BasicResponseDto>($"subscriber/{subscriberGuid}/update-last-sign-in");
        }

        public async Task<List<ImportActionDto>> ImportContactsAsync(Guid partnerGuid, string cacheKey)
        {
            return await PutAsync<List<ImportActionDto>>("contact/import/" + partnerGuid + "/" + HttpUtility.UrlEncode(cacheKey));
        }

        public async Task<bool> IsUserExistsInADB2CAsync(string email)
        {
            var basicResponseDto = await PostAsync<BasicResponseDto>($"identity/is-user-exists-adb2c", new UserDto() { Email = email, Password = string.Empty }, true);
            if (basicResponseDto.StatusCode == 200)
                return true;
            else
                return false;
        }

        public async Task<bool> CreateCustomPasswordResetAsync(string email)
        {
            var basicResponseDto = await PostAsync<BasicResponseDto>($"identity/create-custom-password-reset", new EmailDto() { Email = email }, true);
            if (basicResponseDto.StatusCode == 200)
                return true;
            else
                return false;
        }

        public async Task<bool> ConsumeCustomPasswordResetAsync(Guid passwordResetRequestGuid, string newPassword)
        {
            var basicResponseDto = await PostAsync<BasicResponseDto>($"identity/consume-custom-password-reset", new ResetPasswordDto() { PasswordResetRequestGuid = passwordResetRequestGuid, Password = newPassword }, true);
            if (basicResponseDto.StatusCode == 200)
                return true;
            else
                return false;
        }

        public async Task<bool> CheckValidityOfPasswordResetRequest(Guid passwordResetRequestGuid)
        {
            var basicResponseDto = await GetAsync<BasicResponseDto>($"identity/check-custom-password-reset/" + passwordResetRequestGuid, true);
            if (basicResponseDto.StatusCode == 200)
                return true;
            else
                return false;
        }

        public async Task<bool> IsUserExistsInAuth0Async(string email)
        {
            var basicResponseDto = await PostAsync<BasicResponseDto>($"identity/is-user-exists-auth0", new UserDto() { Email = email, Password = string.Empty }, true);
            if (basicResponseDto.StatusCode == 200)
                return true;
            else
                return false;
        }

        public async Task<bool> CheckADB2CLoginAsync(string email, string password)
        {
            var basicResponseDto = await PostAsync<BasicResponseDto>("identity/check-adb2c-login", new UserDto() { Email = email, Password = password }, true);
            if (basicResponseDto.StatusCode == 200)
                return true;
            else
                return false;
        }

        public async Task<BasicResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var basicResponseDto = await PostAsync<BasicResponseDto>("identity/create-user", createUserDto, true);
            return basicResponseDto;
        }

        public async Task<bool> MigrateUserAsync(CreateUserDto createUserDto)
        {
            var basicResponseDto = await PostAsync<BasicResponseDto>("identity/migrate-user", createUserDto, true);
            if (basicResponseDto.StatusCode == 200)
                return true;
            else
                return false;
        }

        #region Resume Parse 

        public async Task<ResumeParseQuestionnaireDto> GetResumeParseQuestionnaireForSubscriber(Guid subscriberGuid)
        {
            try
            {
                ResumeParseQuestionnaireDto retVal = await GetAsync<ResumeParseQuestionnaireDto>("resume/profile-merge-questionnaire");
                return retVal;
            }
            catch
            {
                return null;
            }
        }


        public async Task<ResumeParseDto> GetResumeParseForSubscriber(Guid subscriberGuid)
        {
            try
            {
                ResumeParseDto retVal = await GetAsync<ResumeParseDto>("resume/resume-parse");
                return retVal;
            }
            catch
            {
                return null;
            }
        }


        public async Task<BasicResponseDto> ResolveResumeParse(Guid resumeParseGuid, string mergeInfo)
        {
            try
            {
                BasicResponseDto retVal = await PostAsync<BasicResponseDto>($"resume/resolve-profile-merge/{resumeParseGuid}", mergeInfo);
                return retVal;
            }
            catch (Exception e)
            {
                string temp = e.Message;
                return null;
            }
        }

        public async Task<FileDto> GetFile(Guid fileDownloadTrackerGuid)
        {
            try
            {
                FileDto retVal = await GetAsync<FileDto>($"file/gated/{fileDownloadTrackerGuid}");
                return retVal;
            }
            catch (Exception e)
            {
                string temp = e.Message;
                return null;
            }
        }


        #endregion

        #region jobs
        public async Task<IList<JobSiteScrapeStatisticDto>> JobScrapeStatisticsSearchAsync(int numRecords)
        {
            return await GetAsync<IList<JobSiteScrapeStatisticDto>>($"job/scrape-statistics/{numRecords}");
        }

        #endregion


        #region Promocode
        public async Task<PromoCodeDto> PromoCodeRedemptionValidationAsync(string promoCodeRedemptionGuid, string courseGuid)
        {
            return await GetAsync<PromoCodeDto>("promocode/redemption-validate/" + promoCodeRedemptionGuid + "/course-variant/" + courseGuid);
        }

        public async Task<PromoCodeDto> PromoCodeValidationAsync(string code, string courseVariantGuid)
        {
            return await GetAsync<PromoCodeDto>("promocode/validate/" + code + "/course-variant/" + courseVariantGuid);
        }

        public async Task<PromoCodeDto> ServiceOfferingPromoCodeValidationAsync(string code, string serviceOfferingGuid)
        {
            return await GetAsync<PromoCodeDto>("promocode/validate/" + code + "/service-offering/" + serviceOfferingGuid);
        }
        #endregion

        #region Enrollment
        public async Task<BraintreeResponseDto> SubmitBraintreePaymentAsync(BraintreePaymentDto BraintreePaymentDto)
        {
            return await PostAsync<BraintreeResponseDto>("enrollment/ProcessBraintreePayment", BraintreePaymentDto);
        }
        public async Task<Guid> EnrollStudentAndObtainEnrollmentGUIDAsync(EnrollmentFlowDto enrollmentFlowDto)
        {
            return await PostAsync<Guid>("enrollment/", enrollmentFlowDto);
        }
        public async Task<CourseLoginDto> CourseLoginAsync(Guid EnrollmentGuid)
        {
            return await GetAsync<CourseLoginDto>($"enrollment/{EnrollmentGuid}/student-login-url");
        }
        #endregion

        #region Skills
        private async Task<IList<SkillDto>> _GetSkillsAsync(string userQuery)
        {
            return await GetAsync<IList<SkillDto>>("skill/" + userQuery);
        }

        public async Task<BasicResponseDto> UpdateEntitySkillsAsync(EntitySkillDto entitySkillDto)
        {
            return await PutAsync<BasicResponseDto>("skill/update", entitySkillDto);
        }

        public async Task<IList<SkillDto>> GetEntitySkillsAsync(string entityType, Guid entityGuid)
        {
            return await GetAsync<IList<SkillDto>>($"skill/get/{entityType}/{entityGuid}");
        }
        #endregion

        #region Subscriber

        public async Task<SubscriberDto> GetSubscriberByGuid(Guid subscriberGuid)
        {
            return await _SubscriberAsync(subscriberGuid);
        }

        public async Task<HttpResponseMessage> DownloadFileAsync(Guid subscriberGuid, Guid fileGuid)
        {
            return await RequestAsync(Constants.HttpGetClientName, HttpMethod.Get, String.Format("subscriber/{0}/file/{1}", subscriberGuid, fileGuid.ToString()));
        }

        public async Task<SubscriberEducationHistoryDto> AddEducationalHistoryAsync(Guid subscriberGuid, SubscriberEducationHistoryDto educationHistory)
        {
            return await PostAsync<SubscriberEducationHistoryDto>(string.Format("subscriber/{0}/education-history", subscriberGuid.ToString()), educationHistory);
        }

        public async Task<BasicResponseDto> AddJobPostingAsync(JobPostingDto jobPosting)
        {
            return await PostAsync<BasicResponseDto>(string.Format("job"), jobPosting);
        }

        public async Task<BasicResponseDto> UpdateJobPostingAsync(JobPostingDto jobPosting)
        {
            return await PutAsync<BasicResponseDto>(string.Format("job"), jobPosting);
        }

        public async Task<List<JobPostingDto>> GetJobPostingsForSubscriber(Guid subscriberGuid)
        {
            return await GetAsync<List<JobPostingDto>>(string.Format("job/subscriber/{0}", subscriberGuid.ToString()));
        }

        public async Task<JobPostingDto> GetJobPostingByGuid(Guid jobPostingGuid)
        {
            // Todo enhance api interface to eliminate catching exceptions - one approach would be to enhance the BasicResponseDto object 
            // with a ResponseObject property which would allow the api to always return a BasicResponseDto object.  The caller code could the check
            // the status of the returned BasicResponseDto and if its 200 then deserialize the ResponseObject into the desired type.  e.g.
            //  return JsonConvert.DeserializeObject<JobPostingDto>(BasicResponseDto.ResponseObject.ToString());
            try
            {
                return await GetAsync<JobPostingDto>(string.Format("job/{0}", jobPostingGuid.ToString()));
            }
            catch
            {
                return null;
            };
        }

        /// <summary>
        /// POST search for getting mapping of job guid favorites for a subscriber.
        /// </summary>
        /// <param name="jobGuids"></param>
        /// <returns>Dictionary<Guid, Guid> JobPosting Guid to JobPostingFavorite Guid</returns>
        public async Task<Dictionary<Guid, Guid>> JobFavoritesByJobGuidAsync(List<Guid> jobGuids)
        {
            return await PostAsync<Dictionary<Guid, Guid>>("subscriber/me/job-favorite/map", jobGuids);
        }

        public async Task<JobPostingDto> CopyJobPosting(Guid jobPostingGuid)
        {
            return await PostAsync<JobPostingDto>(string.Format("job/{0}", jobPostingGuid.ToString()));
        }

        public async Task<BasicResponseDto> DeleteJobPosting(Guid jobPostingGuid)
        {
            return await DeleteAsync<BasicResponseDto>(string.Format("job/{0}", jobPostingGuid.ToString()));
        }

        public async Task<IList<SubscriberEducationHistoryDto>> GetEducationHistoryAsync(Guid subscriberGuid)
        {
            return await GetAsync<IList<SubscriberEducationHistoryDto>>(string.Format("subscriber/{0}/education-history", subscriberGuid.ToString()));
        }

        public async Task<SubscriberEducationHistoryDto> UpdateEducationHistoryAsync(Guid subscriberGuid, SubscriberEducationHistoryDto educationHistory)
        {
            return await PutAsync<SubscriberEducationHistoryDto>(string.Format("subscriber/{0}/education-history", subscriberGuid.ToString()), educationHistory);
        }

        public Task<SubscriberEducationHistoryDto> DeleteEducationHistoryAsync(Guid subscriberGuid, Guid educationHistory)
        {
            return DeleteAsync<SubscriberEducationHistoryDto>(string.Format("subscriber/{0}/education-history/{1}", subscriberGuid.ToString(), educationHistory.ToString()));
        }

        public async Task<BasicResponseDto> UpdateProfileInformationAsync(SubscriberDto Subscriber)
        {
            return await PutAsync<BasicResponseDto>("subscriber", Subscriber);
        }

        public async Task<BasicResponseDto> UpdateOnboardingStatusAsync()
        {
            return await PutAsync<BasicResponseDto>("subscriber/onboard");
        }

        public async Task<bool> DeleteSubscriberAsync(Guid subscriberGuid, Guid cloudIdentifier)
        {
            return await DeleteAsync<bool>($"subscriber/{subscriberGuid}/{cloudIdentifier}");
        }
        public async Task<SubscriberWorkHistoryDto> AddWorkHistoryAsync(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory)
        {
            return await PostAsync<SubscriberWorkHistoryDto>(string.Format("subscriber/{0}/work-history", subscriberGuid.ToString()), workHistory);
        }
        public async Task<SubscriberWorkHistoryDto> UpdateWorkHistoryAsync(Guid subscriberGuid, SubscriberWorkHistoryDto workHistory)
        {
            return await PutAsync<SubscriberWorkHistoryDto>(string.Format("subscriber/{0}/work-history", subscriberGuid.ToString()), workHistory);
        }

        public async Task<IList<SubscriberWorkHistoryDto>> GetWorkHistoryAsync(Guid subscriberGuid)
        {
            return await GetAsync<IList<SubscriberWorkHistoryDto>>(string.Format("subscriber/{0}/work-history", subscriberGuid.ToString()));
        }

        public Task<SubscriberWorkHistoryDto> DeleteWorkHistoryAsync(Guid subscriberGuid, Guid workHistoryGuid)
        {
            return DeleteAsync<SubscriberWorkHistoryDto>(string.Format("subscriber/{0}/work-history/{1}", subscriberGuid.ToString(), workHistoryGuid.ToString()));
        }
        #endregion

        #region Campaign

        public async Task<CampaignPartnerContactDto> GetCampaignPartnerContactAsync(string tinyId)
        {
            return await GetAsync<CampaignPartnerContactDto>($"campaigns/partner-contact/{tinyId}");
        }
        public async Task<IList<CampaignDetailDto>> CampaignDetailsSearchAsync(Guid CampaginGuid)
        {
            return await GetAsync<IList<CampaignDetailDto>>("marketing/campaign-detail/" + CampaginGuid);
        }

        public async Task<IList<CampaignStatisticDto>> CampaignStatisticsSearchAsync()
        {
            return await GetAsync<IList<CampaignStatisticDto>>("marketing/campaign-statistic");
        }

        public async Task<IList<CampaignDto>> GetCampaignsAsync()
        {
            string cacheKey = $"Campaigns";
            IList<CampaignDto> rval = GetCachedValueAsync<IList<CampaignDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _CampaignsAsync();
                SetCachedValueAsync<IList<CampaignDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<CampaignDto> GetCampaignAsync(Guid CampaignGuid)
        {
            IList<CampaignDto> _campaigns = await GetCampaignsAsync();
            foreach (CampaignDto campaign in _campaigns)
            {
                if (campaign.CampaignGuid == CampaignGuid)
                {
                    return campaign;
                }
            }
            return null;
        }

        private async Task<IList<CampaignDto>> _CampaignsAsync()
        {
            return await GetAsync<IList<CampaignDto>>("campaigns");
        }
        #endregion

        #region LinkedIn
        public async Task<LinkedInProfileDto> GetLinkedInProfileAsync()
        {
            return await GetAsync<LinkedInProfileDto>("linkedin");
        }
        #endregion

        public async Task<BasicResponseDto> SyncLinkedInAccountAsync(string linkedInCode, string returnUrl)
        {
            return await PutAsync<BasicResponseDto>($"linkedin/sync-profile/{linkedInCode}?returnUrl={returnUrl}");
        }

        public async Task<WozCourseProgressDto> UpdateStudentCourseProgressAsync(bool FutureSchedule)
        {
            return await PutAsync<WozCourseProgressDto>("course/update-student-course-status/" + FutureSchedule.ToString());
        }

        public async Task<BasicResponseDto> UploadResumeAsync(ResumeDto resumeDto)
        {

            return await PostAsync<BasicResponseDto>("resume/upload", resumeDto);
        }

        public async Task<RedirectDto> GetSubscriberPartnerWebRedirect()
        {
            return await GetAsync<RedirectDto>("subscriber/me/partner-web-redirect");
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

        public async Task<IList<StateDto>> GetAllStatesAsync()
        {
            string cacheKey = $"States";
            IList<StateDto> rval = GetCachedValueAsync<IList<StateDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await GetStatesAsync();
                SetCachedValueAsync<IList<StateDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<ExperienceLevelDto>> _GetExperienceLevelAsync()
        {

            return await GetAsync<IList<ExperienceLevelDto>>("lookupdata/experience-level");
        }

        public async Task<IList<EducationLevelDto>> _GetEducationLevelAsync()
        {

            return await GetAsync<IList<EducationLevelDto>>("lookupdata/education-level");
        }

        public async Task<IList<SecurityClearanceDto>> _GetSecurityClearanceAsync()
        {

            return await GetAsync<IList<SecurityClearanceDto>>("lookupdata/security-clearance");
        }

        public async Task<IList<RecruiterCompanyDto>> _GetRecruiterCompanyAsync(Guid subscriberGuid)
        {

            return await GetAsync<IList<RecruiterCompanyDto>>($"subscriber/{subscriberGuid}/company");
        }

        public async Task<IList<EmploymentTypeDto>> _GetEmploymentTypeAsync()
        {

            return await GetAsync<IList<EmploymentTypeDto>>("lookupdata/employment-type");
        }

        public async Task<IList<CompensationTypeDto>> _GetCompensationTypeAsync()
        {

            return await GetAsync<IList<CompensationTypeDto>>("lookupdata/compensation-type");
        }

        public async Task<List<JobPostingDto>> _GetAllJobsAsync()
        {
            return await GetAsync<List<JobPostingDto>>("sitemap/job");
        }

        public async Task<IList<JobCategoryDto>> _GetJobCategoryAsync()
        {

            return await GetAsync<IList<JobCategoryDto>>("lookupdata/job-category");
        }

        public async Task<IList<IndustryDto>> _GetIndustryAsync()
        {

            return await GetAsync<IList<IndustryDto>>("lookupdata/industry");
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

        private async Task<IList<CompanyDto>> _GetAllCompaniesAsync()
        {
            return await GetAsync<IList<CompanyDto>>("companies/");
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

        public async Task<CourseDto> _GetCourseByCampaignGuid(Guid CampaignGuid)
        {
            return await GetAsync<CourseDto>("course/campaign/" + CampaignGuid);
        }

        public async Task<IList<OfferDto>> _GetOffersAsync()
        {
            return await GetAsync<IList<OfferDto>>("offers");
        }


        #endregion

        #region Private Cache Functions

        private bool SetCachedValueAsync<T>(string CacheKey, T Value, int? cacheTTL = null)
        {
            try
            {

                int CacheTTLMinutes = cacheTTL == null ? int.Parse(_configuration["redis:cacheTTLInMinutes"]) : cacheTTL.Value;
                string newValue = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                _memoryCache.Set(CacheKey, newValue, DateTimeOffset.Now.AddMinutes(CacheTTLMinutes));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool RemoveCachedValue<T>(string CacheKey)
        {
            try
            {
                // _cache.Remove(CacheKey);
                _memoryCache.Remove(CacheKey);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private T GetCachedValueAsync<T>(string CacheKey)
        {
            try
            {

                //string existingValue = await _cache.GetStringAsync(CacheKey);
                string existingValue = _memoryCache.Get<string>(CacheKey);
                if (string.IsNullOrEmpty(existingValue))
                    return (T)Convert.ChangeType(null, typeof(T));
                else
                {
                    T rval = JsonConvert.DeserializeObject<T>(existingValue);
                    return rval;
                }
            }
            catch (Exception)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }

        #endregion

        #region TalentPortal

        public async Task<SubscriberDto> SubscriberAsync(Guid subscriberGuid, bool hardRefresh)
        {
            string cacheKey = $"Subscriber{subscriberGuid}";
            SubscriberDto rval = null;
            if (!hardRefresh)
                rval = GetCachedValueAsync<SubscriberDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _SubscriberAsync(subscriberGuid);
                SetCachedValueAsync<SubscriberDto>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<List<SubscriberInitialSourceDto>> NewSubscribersCSVAsync()
        {
            string endpoint = $"report/new-subscriber-csv";
            return await GetAsync<List<SubscriberInitialSourceDto>>(endpoint);
        }



        public async Task<ProfileSearchResultDto> SubscriberSearchAsync(string searchFilter, string searchQuery, string searchLocationQuery, string sortOrder)
        {
            string endpoint = $"subscriber/search?searchFilter={searchFilter}";
            if (searchQuery != string.Empty)
                endpoint += $"&searchQuery={searchQuery}";
            if (searchLocationQuery != string.Empty)
                endpoint += $"&searchLocationQuery={searchLocationQuery}";
            if (sortOrder != string.Empty)
                endpoint += $"&sortOrder={WebUtility.UrlEncode(sortOrder)}";



            return await GetAsync<ProfileSearchResultDto>(endpoint);
        }

        public async Task<IList<SubscriberSourceStatisticDto>> SubscriberSourcesAsync()
        {
            return await GetAsync<IList<SubscriberSourceStatisticDto>>("subscriber/sources");
        }

        private async Task<SubscriberDto> _SubscriberAsync(Guid subscriberGuid)
        {
            return await GetAsync<SubscriberDto>($"subscriber/{subscriberGuid}");
        }

        private async Task<ContactDto> _Contact(Guid contactGuid)
        {
            return await GetAsync<ContactDto>($"contact/{contactGuid}");
        }

        public async Task<BasicResponseDto> SaveNotes(SubscriberNotesDto subscriberNotesDto)
        {
            return await PostAsync<BasicResponseDto>("subscriber/save-notes", subscriberNotesDto);
        }

        public async Task<IList<SubscriberNotesDto>> SubscriberNotesSearch(string subscriberGuid, string searchQuery)
        {
            string endpoint = $"subscriber/notes/{subscriberGuid}";
            if (searchQuery != string.Empty)
                endpoint += $"?searchQuery={searchQuery}";

            return await GetAsync<IList<SubscriberNotesDto>>(endpoint);
        }

        public async Task<bool> DeleteNoteAsync(Guid subscriberNotesGuid)
        {
            return await DeleteAsync<bool>($"subscriber/notes/{subscriberNotesGuid}");
        }

        #endregion

        #region AdminPortal
        public async Task<SubscriberReportDto> GetSubscriberReportAsync(List<DateTime> dates = null)
        {
            string query = string.Empty;
            if (dates.Any())
            {
                query += "?dates=" + string.Join("&dates=", dates);
            }

            return await GetAsync<SubscriberReportDto>($"report/subscribers{query}");
        }

        public async Task<List<SubscriberSignUpCourseEnrollmentStatisticsDto>> GetSubscriberReportByPartnerAsync()
        {
            return await GetAsync<List<SubscriberSignUpCourseEnrollmentStatisticsDto>>($"report/partners");
        }

        public async Task<List<JobApplicationCountDto>> GetJobApplicationCount(Guid? companyGuid = null)
        {
            string endpoint = "/api/report/job-applications";
            if (companyGuid.HasValue)
                endpoint += string.Format("/company/{0}", companyGuid.Value);
            return await GetAsync<List<JobApplicationCountDto>>(endpoint);
        }


        public async Task<List<NotificationCountsReportDto>> GetReadNotificationsCount()
        {
            string endpoint = "/api/report/notification-reads";
            return await GetAsync<List<NotificationCountsReportDto>>(endpoint);
        }

        public async Task<List<RecruiterActionSummaryDto>> GetRecruiterActionSummaryAsync()
        {
            return await GetAsync<List<RecruiterActionSummaryDto>>($"report/recruiter-action-summary");
        }
        public async Task<List<SubscriberActionSummaryDto>> GetSubscriberActionSummaryAsync()
        {
            return await GetAsync<List<SubscriberActionSummaryDto>>($"report/subscriber-action-summary");
        }
        public async Task<List<OfferActionSummaryDto>> GetOfferActionSummaryAsync()
        {
            return await GetAsync<List<OfferActionSummaryDto>>($"report/offer-action-summary");
        }
        public async Task<ActionReportDto> GetPartnerSubscriberActionStatsAsync()
        {
            return await GetAsync<ActionReportDto>("report/subscriber-actions");
        }

        public async Task<IList<PartnerDto>> GetPartnersAsync()
        {
            string cacheKey = $"Partners";
            IList<PartnerDto> rval = GetCachedValueAsync<IList<PartnerDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _PartnersAsync();
                SetCachedValueAsync<IList<PartnerDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<PartnerDto> GetPartnerAsync(Guid PartnerGuid)
        {
            IList<PartnerDto> _partners = await GetPartnersAsync();
            foreach (PartnerDto partner in _partners)
            {
                if (partner.PartnerGuid == PartnerGuid)
                {
                    return partner;
                }
            }
            return null;
        }

        public async Task<PartnerDto> GetPartnerByNameAsync(string partnerName)
        {
            IList<PartnerDto> _partners = await GetPartnersAsync();
            foreach (PartnerDto partner in _partners)
            {
                if (partner.Name == partnerName)
                {
                    return partner;
                }
            }
            return null;
        }





        private async Task<IList<PartnerDto>> _PartnersAsync()
        {
            return await GetAsync<IList<PartnerDto>>("partners");
        }

        public async Task<PartnerDto> CreatePartnerAsync(PartnerDto partnerDto)
        {
            // Create the new partner and store it for return
            PartnerDto newPartner = await PostAsync<PartnerDto>("partners", partnerDto);

            // Reset the cached partners list to contain the new partner
            string cacheKey = $"Partners";
            RemoveCachedValue<IList<PartnerDto>>(cacheKey);

            // Return the newly created partner
            return newPartner;
        }

        public async Task<BasicResponseDto> UpdatePartnerAsync(PartnerDto partnerDto)
        {
            // Update partner
            BasicResponseDto updatedPartnerResponse = await PutAsync<BasicResponseDto>("partners", partnerDto);

            // Reset the cached partners list to contain the new partner
            string cacheKey = $"Partners";
            RemoveCachedValue<IList<PartnerDto>>(cacheKey);

            // Return the newly created partner
            return updatedPartnerResponse;

        }


        public async Task<BasicResponseDto> DeletePartnerAsync(Guid partnerGuid)
        {
            // Update partner
            BasicResponseDto deletedPartnerResponse = await DeleteAsync<BasicResponseDto>(string.Format("partners/{0}", partnerGuid));

            // Reset the cached partners list to contain the new partner
            string cacheKey = $"Partners";
            RemoveCachedValue<IList<PartnerDto>>(cacheKey);

            // Return the newly created partner
            return deletedPartnerResponse;
        }

        public async Task<List<JobPostingCountReportDto>> GetActiveJobPostCountPerCompanyByDatesAsynch(DateTime? startPostDate = null, DateTime? endPostDate = null)
        {
            string query = string.Empty;
            if (startPostDate.HasValue)
            {
                query += string.Join("?startPostDate=", startPostDate.Value);
            }

            if (endPostDate.HasValue)
            {
                query += string.Join("&endPostDate=", startPostDate.Value);
            }

            return await GetAsync<List<JobPostingCountReportDto>>($"report/job-post-count{query}");
        }

        public async Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync()
        {
            string cacheKey = $"job-PostCount";
            List<JobPostingCountDto> rval = GetCachedValueAsync<List<JobPostingCountDto>>(cacheKey);
            if (rval == null)
            {
                int cacheTTL = int.Parse(_configuration["USMaps:cacheTTLInMinutes"]);
                rval = await GetAsync<List<JobPostingCountDto>>($"job/post-count");
                SetCachedValueAsync<List<JobPostingCountDto>>(cacheKey, rval, cacheTTL);
            }
            return rval;
        }

        public async Task<IList<NotificationDto>> GetNotificationsAsync()
        {
            string cacheKey = $"Notifications";
            IList<NotificationDto> rval = GetCachedValueAsync<IList<NotificationDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _NotificationsAsync();
                SetCachedValueAsync<IList<NotificationDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<NotificationDto> GetNotificationAsync(Guid NotificationGuid)
        {
            IList<NotificationDto> _notifications = await GetNotificationsAsync();
            foreach (NotificationDto notification in _notifications)
            {
                if (notification.NotificationGuid == NotificationGuid)
                {
                    return notification;
                }
            }
            return null;
        }

        private async Task<IList<NotificationDto>> _NotificationsAsync()
        {
            return await GetAsync<IList<NotificationDto>>("notifications");
        }

        public async Task<NewNotificationDto> CreateNotificationAsync(NewNotificationDto newNotificationDto)
        {
            NewNotificationDto newNotification = await PostAsync<NewNotificationDto>("notifications", newNotificationDto);

            string cacheKey = $"Notifications";
            RemoveCachedValue<IList<NotificationDto>>(cacheKey);

            return newNotification;
        }

        public async Task<BasicResponseDto> UpdateNotificationAsync(NotificationDto notificationDto)
        {
            // Update partner
            BasicResponseDto updatedNotificationResponse = await PutAsync<BasicResponseDto>("notifications", notificationDto);

            // Reset the cached partners list to contain the new partner
            string cacheKey = $"Notifications";
            RemoveCachedValue<IList<NotificationDto>>(cacheKey);

            // Return the newly created partner
            return updatedNotificationResponse;

        }


        public async Task<BasicResponseDto> DeleteNotificationAsync(Guid notificationGuid)
        {
            // Update partner
            BasicResponseDto deletedNotificationResponse = await DeleteAsync<BasicResponseDto>(string.Format("notifications/{0}", notificationGuid));

            // Reset the cached partners list to contain the new partner
            string cacheKey = $"Notifications";
            RemoveCachedValue<IList<NotificationDto>>(cacheKey);

            // Return the newly created partner
            return deletedNotificationResponse;
        }

        public async Task<BasicResponseDto> UpdateSubscriberNotificationAsync(Guid SubscriberGuid, NotificationDto notificationDto)
        {
            // Update partner
            BasicResponseDto updatedSubscriberNotificationResponse = await PutAsync<BasicResponseDto>("subscriber/read-notification", notificationDto);

            // Reset the cached partners list to contain the new partner
            string cacheKey = $"Subscriber{SubscriberGuid}";
            RemoveCachedValue<IList<SubscriberDto>>(cacheKey);

            // Return the newly created partner
            return updatedSubscriberNotificationResponse;

        }

        public async Task<BasicResponseDto> DeleteSubscriberNotificationAsync(Guid SubscriberGuid, NotificationDto notificationDto)
        {
            BasicResponseDto deletedSubscriberNotificationResponse = await DeleteAsync<BasicResponseDto>($"subscriber/delete-notification/{notificationDto.NotificationGuid}");
            return deletedSubscriberNotificationResponse;
        }

        public async Task<BasicResponseDto> ToggleSubscriberNotificationEmailAsync(Guid subscriberGuid, bool isEnabled)
        {
            BasicResponseDto toggledSubscriberNotificationEmailResponse = await PutAsync<BasicResponseDto>($"subscriber/{subscriberGuid}/toggle-notification-emails/{isEnabled.ToString()}");
            return toggledSubscriberNotificationEmailResponse;
        }

        public async Task<List<FailedSubscriberDto>> GetFailedSubscribersSummaryAsync()
        {
            return await GetAsync<List<FailedSubscriberDto>>("subscriber/failed-subscribers");
        }

        public async Task<List<GroupDto>> GetGroupsAsync(){
            return await GetAsync<List<GroupDto>>("groups");
        }

        #endregion

        #region JobBoard


        private async Task<IList<SearchTermDto>> _GetKeywordSearchTermsAsync()
        {
            return await GetAsync<IList<SearchTermDto>>("job/keyword-search-terms");
        }

        private async Task<IList<SearchTermDto>> _GetLocationSearchTermsAsync()
        {
            return await GetAsync<IList<SearchTermDto>>("job/location-search-terms");
        }

        public async Task<IList<SearchTermDto>> GetKeywordSearchTermsAsync(string value)
        {
            string cacheKey = $"GetKeywordSearchTerms";
            IList<SearchTermDto> rval = GetCachedValueAsync<IList<SearchTermDto>>(cacheKey);

            if (rval == null)
            {
                rval = await _GetKeywordSearchTermsAsync();
                SetCachedValueAsync<IList<SearchTermDto>>(cacheKey, rval);
            }

            return rval?.Where(v => v.Value.Contains(value))?.ToList();
        }

        public async Task<IList<SearchTermDto>> GetLocationSearchTermsAsync(string value)
        {
            string cacheKey = $"GetLocationSearchTerms";
            IList<SearchTermDto> rval = GetCachedValueAsync<IList<SearchTermDto>>(cacheKey);

            if (rval == null)
            {
                rval = await _GetLocationSearchTermsAsync();
                SetCachedValueAsync<IList<SearchTermDto>>(cacheKey, rval);
            }

            return rval?.Where(v => v.Value.Contains(value))?.ToList();
        }

        public async Task<JobPostingDto> _GetJobAsync(Guid JobPostingGuid)
        {
            return await GetAsync<JobPostingDto>("job/" + JobPostingGuid);
        }

        public async Task<BasicResponseDto> ApplyToJobAsync(JobApplicationDto JobApplication)
        {
            return await PostAsync<BasicResponseDto>("jobApplication", JobApplication);
        }

        public async Task<JobSearchResultDto> _GetJobsByLocation(string searchFilter)
        {
            return await GetAsync<JobSearchResultDto>("job/browse-jobs-location/" + searchFilter);
        }

        public async Task<GoogleCloudEventsTrackingDto> RecordClientEventAsync(Guid jobGuid, GoogleCloudEventsTrackingDto dto)
        {
            // covenience check to allow callers to not have to check dto for null
            if (dto == null)
                return null;

            // this is purely for analytics purposes if this call fails we don't need to break anything else calling it
            try
            {
                return await PostAsync<GoogleCloudEventsTrackingDto>(string.Format("job/{0}/track", jobGuid), dto);
            }
            catch (Exception)
            {
                // todo: add logger
                return null;
            }
        }

        public async Task<JobPostingDto> GetExpiredJobAsync(Guid JobPostingGuid)
        {
            try
            {
                return await GetAsync<JobPostingDto>("job/expired/" + JobPostingGuid);
            }
            catch (ApiException ae)
            {
                //todo: add logger
                if (ae.ResponseDto.StatusCode == 404)
                {
                    // Null result is possible if job GUID doesn't exist in our repo.
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }


        public async Task<IList<JobCategoryDto>> GetJobCategories()
        {
            string cacheKey = $"JobCategories";
            IList<JobCategoryDto> rval = GetCachedValueAsync<IList<JobCategoryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetJobCategories();
                SetCachedValueAsync<IList<JobCategoryDto>>(cacheKey, rval);
            }
            return rval;
        }

        private async Task<IList<JobCategoryDto>> _GetJobCategories()
        {
            return await GetAsync<IList<JobCategoryDto>>("job/categories");
        }

        public async Task ReferJobPosting(string jobPostingId, string referrerGuid, string refereeName, string refereeEmailId, string descriptionEmailBody)
        {
            //refer Url
            var referUrl = $"{_configuration["Environment:BaseUrl"].TrimEnd('/')}/job/{Guid.Parse(jobPostingId)}";

            JobReferralDto jobReferralDto = new JobReferralDto()
            {
                JobPostingGuidStr = jobPostingId,
                ReferrerGuid = referrerGuid,
                RefereeName = refereeName,
                RefereeEmailId = refereeEmailId,
                DescriptionEmailBody = descriptionEmailBody,
                ReferUrl = referUrl
            };


            await PostAsync<BasicResponseDto>("job/referral", jobReferralDto);
        }

        public async Task UpdateJobReferral(string referrerCode, Guid subscriberGuid)
        {
            await PutAsync<BasicResponseDto>("job/update-referral", new JobReferralDto { JobReferralGuid = referrerCode, RefereeGuid = subscriberGuid.ToString() });
        }

        public async Task UpdateJobViewed(string referrerCode)
        {
            await PutAsync<BasicResponseDto>("job/update-job-viewed", referrerCode);
        }

        public async Task<BasicResponseDto> _GetActiveJobCountAsync()
        {
            return await GetAsync<BasicResponseDto>("job/active-job-count");
        }

        public async Task RecordSubscriberApplyAction(Guid jobGuid, Guid subscriberGuid)
        {
            await GetAsync<BasicResponseDto>($"tracking/record-subscriber-apply-action/{jobGuid}/{subscriberGuid}");
        }

        public async Task RecordSubscriberJobViewAction(Guid jobGuid, Guid subscriberGuid)
        {
            await GetAsync<BasicResponseDto>($"tracking/track-subscriber-job-view-action/{jobGuid}/{subscriberGuid}");
        }

        #endregion

        #region Traitify

        public async Task<TraitifyDto> StartNewTraitifyAssessment(TraitifyDto dto)
        {
            return await PostAsync<TraitifyDto>("traitify/new", dto);
        }

        public async Task<TraitifyDto> GetTraitifyByAssessmentId(string assessmentId)
        {
            return await GetAsync<TraitifyDto>($"traitify/{assessmentId}");
        }

        public async Task<TraitifyDto> CompleteAssessment(string assessmentId)
        {
            return await GetAsync<TraitifyDto>($"traitify/complete/{assessmentId}");
        }

        public async Task<BasicResponseDto> AssociateSubscriberWithAssessment(string assessmentId, Guid subscriberGuid)
        {
            return await PutAsync<BasicResponseDto>($"traitify/{assessmentId}/subscriber/{subscriberGuid}");
        }

        #endregion
    }
}