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
using System.Threading;
using UpDiddyLib.Dto.Reporting;

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
        public HttpContext _currentContext { get; set; }

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
            _currentContext = contextAccessor.HttpContext;
        }
        #endregion

        #region Request Methods
        private async Task<HttpResponseMessage> RequestAsync(string clientName, HttpMethod method, string endpoint, object body = null)
        {
            HttpClient client = _HttpClientFactory.CreateClient(clientName);
            client.BaseAddress = new Uri(_ApiBaseUri);

            client = await AddBearerTokenAsync(client);

            HttpRequestMessage request = new HttpRequestMessage(method, endpoint);

            if (body != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(body));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return await client.SendAsync(request);
        }

        private async Task<T> RequestAsync<T>(string clientName, HttpMethod method, string endpoint, object body = null)
        {
            using (var response = await RequestAsync(clientName, method, endpoint, body))
            {
                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

                throw new ApiException(response, JsonConvert.DeserializeObject<BasicResponseDto>(await response.Content.ReadAsStringAsync()));
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            return await RequestAsync<T>(Constants.HttpGetClientName, HttpMethod.Get, endpoint);
        }

        public async Task<T> PostAsync<T>(string endpoint, object body = null)
        {
            return await RequestAsync<T>(Constants.HttpPostClientName, HttpMethod.Post, endpoint, body);
        }

        public async Task<T> PutAsync<T>(string endpoint, object body = null)
        {
            return await RequestAsync<T>(Constants.HttpPutClientName, HttpMethod.Put, endpoint, body);
        }

        public async Task<T> DeleteAsync<T>(string endpoint)
        {
            return await RequestAsync<T>(Constants.HttpDeleteClientName, HttpMethod.Delete, endpoint);
        }

        private async Task<AuthenticationResult> GetBearerTokenAsync()
        {
            // Retrieve the token with the specified scopes
            var scope = AzureOptions.ApiScopes.Split(' ');
            string signedInUserID = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(AzureOptions.ClientId)
                .WithB2CAuthority(AzureOptions.Authority)
                .WithClientSecret(AzureOptions.ClientSecret)
                .Build();
            new MSALSessionCache(signedInUserID, _contextAccessor.HttpContext).EnablePersistence(app.UserTokenCache);
            var accounts = await app.GetAccountsAsync();

            AuthenticationResult result = await app.AcquireTokenSilent(scope, accounts.FirstOrDefault()).ExecuteAsync();
            return result;
        }

        private async Task<HttpClient> AddBearerTokenAsync(HttpClient client)
        {
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                AuthenticationResult authResult = await GetBearerTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            }
            return client;
        }
        #endregion

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



        public async Task<IList<ExperienceLevelDto>> GetExperienceLevelAsync()
        {
            string cacheKey = "GetExperienceLevelAsync";
            IList<ExperienceLevelDto> rval = GetCachedValue<IList<ExperienceLevelDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetExperienceLevelAsync();
                SetCachedValue<IList<ExperienceLevelDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<SecurityClearanceDto>> GetSecurityClearanceAsync()
        {
            string cacheKey = "GetSecurityClearanceAsync";
            IList<SecurityClearanceDto> rval = GetCachedValue<IList<SecurityClearanceDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetSecurityClearanceAsync();
                SetCachedValue<IList<SecurityClearanceDto>>(cacheKey, rval);
            }
            return rval;
        }


        public async Task<IList<RecruiterCompanyDto>> GetRecruiterCompaniesAsync(Guid subscriberGuid)
        {
            string cacheKey = $"GetRecruiterCompaniesAsync{subscriberGuid}";
            IList<RecruiterCompanyDto> rval = GetCachedValue<IList<RecruiterCompanyDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetRecruiterCompanyAsync(subscriberGuid);
                SetCachedValue<IList<RecruiterCompanyDto>>(cacheKey, rval);
            }
            return rval;
        }








        public async Task<IList<EmploymentTypeDto>> GetEmploymentTypeAsync()
        {
            string cacheKey = "GetEmploymentTypeAsync";
            IList<EmploymentTypeDto> rval = GetCachedValue<IList<EmploymentTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEmploymentTypeAsync();
                SetCachedValue<IList<EmploymentTypeDto>>(cacheKey, rval);
            }
            return rval;
        }



        public async Task<IList<CompensationTypeDto>> GetCompensationTypeAsync()
        {
            string cacheKey = "GetCompensationTypeAsync";
            IList<CompensationTypeDto> rval = GetCachedValue<IList<CompensationTypeDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCompensationTypeAsync();
                SetCachedValue<IList<CompensationTypeDto>>(cacheKey, rval);
            }
            return rval;
        }




        public async Task<IList<EducationLevelDto>> GetEducationLevelAsync()
        {
            string cacheKey = "GetEducationLevelAsync";
            IList<EducationLevelDto> rval = GetCachedValue<IList<EducationLevelDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetEducationLevelAsync();
                SetCachedValue<IList<EducationLevelDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<List<JobPostingDto>> GetAllJobsAsync()
        {
            string cacheKey = "GetAllJobsAsync";
            List<JobPostingDto> rval = GetCachedValue<List<JobPostingDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetAllJobsAsync();
                SetCachedValue<List<JobPostingDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<JobCategoryDto>> GetJobCategoryAsync()
        {
            string cacheKey = "GetJobCategoryAsync";
            IList<JobCategoryDto> rval = GetCachedValue<IList<JobCategoryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetJobCategoryAsync();
                SetCachedValue<IList<JobCategoryDto>>(cacheKey, rval);
            }
            return rval;
        }



        public async Task<IList<IndustryDto>> GetIndustryAsync()
        {
            string cacheKey = "GetIndustryAsync";
            IList<IndustryDto> rval = GetCachedValue<IList<IndustryDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetIndustryAsync();
                SetCachedValue<IList<IndustryDto>>(cacheKey, rval);
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

        public async Task<CourseDto> GetCourseByCampaignGuidAsync(Guid CampaignGuid)
        {
            string cacheKey = $"GetCourseByCampaignGuid{CampaignGuid}";
            CourseDto rval = GetCachedValue<CourseDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetCourseByCampaignGuid(CampaignGuid);
                SetCachedValue<CourseDto>(cacheKey, rval);
            }
            return rval;

        }

        public async Task<ContactDto> ContactAsync(Guid partnerContactGuid)
        {
            string cacheKey = $"Contact{partnerContactGuid}";
            ContactDto rval = GetCachedValue<ContactDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _Contact(partnerContactGuid);
                SetCachedValue<ContactDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<OfferDto>> GetOffersAsync()
        {
            string cacheKey = $"Offers";
            IList<OfferDto> rval = GetCachedValue<IList<OfferDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _GetOffersAsync();
                SetCachedValue<IList<OfferDto>>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<JobPostingDto> GetJobAsync(Guid JobPostingGuid, GoogleCloudEventsTrackingDto dto = null)
        {
            string cacheKey = $"job-{JobPostingGuid}";
            JobPostingDto rval = GetCachedValue<JobPostingDto>(cacheKey);
            if (rval == null)
            {
                rval = await _GetJobAsync(JobPostingGuid);
                SetCachedValue<JobPostingDto>(cacheKey, rval);
            }

            #region analytics
            GoogleCloudEventsTrackingDto eventDto = null;
            if (dto != null)
                eventDto = await RecordClientEventAsync(JobPostingGuid, dto);

            rval.RequestId = eventDto?.RequestId;
            rval.ClientEventId = eventDto?.ClientEventId;
            #endregion

            return rval;
        }

        public async Task<JobSearchResultDto> GetJobsByLocation(string keywords, string location)
        {
            //job search criteria for api "/country/state/city/industry/job-category/skill/page-num" and query strings appended


            var searchFilter = $"all/all/all/all/all/all/0?page-size=100&location={location}&keywords={keywords}&page-num=0";
            string cacheKey = $"job-{keywords}/{location}";
            JobSearchResultDto rval = GetCachedValue<JobSearchResultDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {

                rval = await _GetJobsByLocation(searchFilter);
                SetCachedValue<JobSearchResultDto>(cacheKey, rval);
            }

            return rval;
        }

        #endregion

        #region Public UnCached Methods

        public async Task<List<ImportActionDto>> ImportContactsAsync(Guid partnerGuid, string cacheKey)
        {
            return await PutAsync<List<ImportActionDto>>("contact/import/" + partnerGuid + "/" + HttpUtility.UrlEncode(cacheKey));
        }


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
        public async Task<BasicResponseDto> UpdateSubscriberContactAsync(Guid partnerContactGuid, SignUpDto signUpDto)
        {
            // encrypt password before sending to API
            signUpDto.password = Crypto.Encrypt(_configuration["Crypto:Key"], signUpDto.password);

            return await PutAsync<BasicResponseDto>(string.Format("subscriber/contact/{0}", partnerContactGuid.ToString()), signUpDto);
        }

        public async Task<BasicResponseDto> ExpressUpdateSubscriberContactAsync(SignUpDto signUpDto)
        {
            // encrypt password before sending to API
            signUpDto.password = Crypto.Encrypt(_configuration["Crypto:Key"], signUpDto.password);

            return await PostAsync<BasicResponseDto>("subscriber/express-sign-up", signUpDto);
        }

        public async Task<SubscriberADGroupsDto> MyGroupsAsync()
        {
            return await GetAsync<SubscriberADGroupsDto>("subscriber/me/group");
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

        public async Task<SubscriberDto> CreateSubscriberAsync(string referralCode)
        {
            return await PostAsync<SubscriberDto>("subscriber", referralCode);
        }
        public async Task<bool> DeleteSubscriberAsync(Guid subscriberGuid)
        {
            return await DeleteAsync<bool>($"subscriber/{subscriberGuid}");
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
            IList<CampaignDto> rval = GetCachedValue<IList<CampaignDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _CampaignsAsync();
                SetCachedValue<IList<CampaignDto>>(cacheKey, rval);
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

        #region EmailVerification
        public async Task<BasicResponseDto> VerifyEmailAsync(Guid token)
        {
            return await PostAsync<BasicResponseDto>($"subscriber/verify-email/{token}");
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

        private bool RemoveCachedValue<T>(string CacheKey)
        {
            try
            {
                _cache.Remove(CacheKey);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region TalentPortal

        public async Task<SubscriberDto> SubscriberAsync(Guid subscriberGuid, bool hardRefresh)
        {
            string cacheKey = $"Subscriber{subscriberGuid}";
            SubscriberDto rval = null;
            if (!hardRefresh)
                rval = GetCachedValue<SubscriberDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _SubscriberAsync(subscriberGuid);
                if (rval == null)
                {
                    //check if there is any referralCode
                    var referralCode=_contextAccessor.HttpContext.Request.Cookies["referrerCode"] == null ? null : _contextAccessor.HttpContext.Request.Cookies["referrerCode"].ToString();
                    rval = await CreateSubscriberAsync(referralCode);
                }
                   
                SetCachedValue<SubscriberDto>(cacheKey, rval);
            }
            return rval;
        }

        public async Task<IList<SubscriberDto>> SubscriberSearchAsync(string searchFilter, string searchQuery)
        {
            string endpoint = $"subscriber/search?searchFilter={searchFilter}";
            if (searchQuery != string.Empty)
                endpoint += $"&searchQuery={searchQuery}";

            return await GetAsync<IList<SubscriberDto>>(endpoint);
        }

        public async Task<IList<SubscriberSourceDto>> SubscriberSourcesAsync()
        {
            return await GetAsync<IList<SubscriberSourceDto>>("subscriber/sources");
        }

        private async Task<SubscriberDto> _SubscriberAsync(Guid subscriberGuid)
        {
            return await GetAsync<SubscriberDto>($"subscriber/{subscriberGuid}");
        }

        private async Task<ContactDto> _Contact(Guid contactGuid)
        {
            return await GetAsync<ContactDto>($"contact/{contactGuid}");
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

        public async Task<SubscriberReportDto> GetSubscriberReportByPartnerAsync()
        {
            return await GetAsync<SubscriberReportDto>($"report/partners");
        }

        public async Task<List<JobApplicationCountDto>> GetJobApplicationCount(Guid? companyGuid = null)
        {
            string endpoint = "/api/report/job-applications";
            if(companyGuid.HasValue)
                endpoint += string.Format("/company/{0}", companyGuid.Value);
            return await GetAsync<List<JobApplicationCountDto>>(endpoint);
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
            IList<PartnerDto> rval = GetCachedValue<IList<PartnerDto>>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = await _PartnersAsync();
                SetCachedValue<IList<PartnerDto>>(cacheKey, rval);
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

        #endregion

        #region JobBoard

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
            catch(Exception e)
            {
                // todo: add logger
                return null;
            }
        }
        
        public async Task<JobPostingDto> GetExpiredJobAsync(Guid JobPostingGuid)
        {
            return await GetAsync<JobPostingDto>("job/expired/" + JobPostingGuid);
        }

        public async Task ReferJobPosting(string jobPostingId, string referrerGuid, string refereeName, string refereeEmailId, string descriptionEmailBody)
        {
            //refer Url
            var referUrl = $"{_configuration["Environment:BaseUrl"].TrimEnd('/')}/jobs/{Guid.Parse(jobPostingId)}";

            JobReferralDto jobReferralDto = new JobReferralDto()
            {
                JobPostingId = jobPostingId,
                ReferrerGuid = referrerGuid,
                RefereeName=refereeName,
                RefereeEmailId = refereeEmailId,
                DescriptionEmailBody = descriptionEmailBody,
                ReferUrl= referUrl
            };


            await PostAsync<BasicResponseDto>("job/referral", jobReferralDto);
        }

        public async Task UpdateJobReferral(string referrerCode, Guid subscriberGuid)
        {
            await PutAsync<BasicResponseDto>("job/update-referral", new JobReferralDto{ JobReferralGuid = referrerCode, RefereeGuid = subscriberGuid.ToString() });
        }

        public async Task UpdateJobViewed(string referrerCode)
        {
            await PutAsync<BasicResponseDto>("job/update-job-viewed",referrerCode);
        }

        #endregion
    }
}