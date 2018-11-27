using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UpDiddy.Helpers;
using UpDiddyLib.Dto;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Polly.Registry;
using Polly;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace UpDiddy.Api
{
    public class ApiUpdiddy : ApiHelperMsal
    {

        #region Constructor
        public ApiUpdiddy(AzureAdB2COptions Options, HttpContext Context, IConfiguration conifguration, IHttpClientFactory httpClientFactory, IDistributedCache cache) {

            AzureOptions = Options;
            HttpContext = Context;
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
        

        public WozCourseProgress GetCurrentCourseProgress(Guid SubscriberGuid, Guid EnrollmentGuid)
        {
            string cacheKey = $"GetCurrentCourseProgress{SubscriberGuid}{EnrollmentGuid}";
            WozCourseProgress rval = GetCachedValue<WozCourseProgress>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetCurrentCourseProgress(SubscriberGuid, EnrollmentGuid);
                SetCachedValue<WozCourseProgress>(cacheKey, rval);
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
        public IList<StateDto> GetStatesByCountry(Guid countryGuid)
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



        public CountryDto GetSubscriberCountry(int StateId)
        {
            string cacheKey = $"GetSubscriberCountry{StateId}";
            CountryDto rval = GetCachedValue<CountryDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetSubscriberCountry(StateId);
                SetCachedValue<CountryDto>(cacheKey, rval);
            }
            return rval;

        }

        public StateDto GetSubscriberState(int StateId)
        {
            string cacheKey = $"GetSubscriberState{StateId}";
            StateDto rval = GetCachedValue<StateDto>(cacheKey);

            if (rval != null)
                return rval;
            else
            {
                rval = _GetSubscriberState(StateId);
                SetCachedValue<StateDto>(cacheKey, rval);
            }
            return rval;
        }


        #endregion

        #region Public UnCached Methods

        public SubscriberDto Subscriber(Guid SubscriberGuid)
        {
            return Get<SubscriberDto>("subscriber/" + SubscriberGuid, true);
        }

        public PromoCodeDto PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseGuid, string subscriberGuid)
        {
            return Get<PromoCodeDto>("promocode/promocoderedemptionvalidation/" + promoCodeRedemptionGuid + "/" + courseGuid + "/" + subscriberGuid, true);
        }

        public PromoCodeDto PromoCodeValidation(string code, string courseVariantGuid, string subscriberGuid)
        {
            return Get<PromoCodeDto>("promocode/" + code + "/" + courseVariantGuid + "/" + subscriberGuid, true);
        }

        public IList<EnrollmentDto> GetCurrentEnrollmentsForSubscriber(SubscriberDto Subscriber)
        {
            return Get<IList<EnrollmentDto>>("enrollment/CurrentEnrollments/" + Subscriber.SubscriberId, true);
        }

        public VendorStudentLoginDto StudentLogin(int SubscriberId)
        {
            return Get<VendorStudentLoginDto>("enrollment/StudentLogin/" + SubscriberId.ToString(), true);
        }

        public CourseLoginDto CourseLogin(Guid SubscriberGuid, Guid CourseGuid, Guid VendorGuid)
        {
            return Get<CourseLoginDto>($"course/StudentLoginUrl/{SubscriberGuid}/{CourseGuid}/{VendorGuid}", true);
        }


        public BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber)
        {
            return Post<BasicResponseDto>(Subscriber, "profile/update", true);
        }

        public BasicResponseDto SyncLinkedInAccount(Guid SubscriberGuid, string linkedInCode)
        {
            return Get<BasicResponseDto>($"/linkedin/SyncProfile/{SubscriberGuid}/{linkedInCode}");
        }

        public Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentFlowDto enrollmentFlowDto)
        {
            return Post<Guid>(enrollmentFlowDto, "enrollment/", true);
        }

        public Guid WriteToEnrollmentLog(EnrollmentLogDto enrollmentLogDto)
        {
            return Post<Guid>(enrollmentLogDto, "enrollment/EnrollmentLog", true);
        }

        public SubscriberDto CreateSubscriberDeprecated(string SubscriberGuid, string SubscriberEmail)
        {
            return Post<SubscriberDto>("subscriber/addsubscriber/" + SubscriberGuid + "/" + Uri.EscapeDataString(SubscriberEmail), true);
        }

        public SubscriberDto CreateSubscriber(string SubscriberGuid, string SubscriberEmail)
        {

            SubscriberCreateDto SDto = new SubscriberCreateDto
            {
                SubscriberGuid = SubscriberGuid,
                SubscriberEmail = SubscriberEmail

            };

            string jsonToSend = JsonConvert.SerializeObject(SDto);
            return Post<SubscriberDto>(SDto, "subscriber/createsubscriber", true);
        }

        public WozCourseProgress UpdateStudentCourseProgress(Guid SubscriberGuid, bool FutureSchedule)
        {

            return Put<WozCourseProgress>("woz/UpdateStudentCourseStatus/" + SubscriberGuid + "/" + FutureSchedule.ToString(), true);
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
        public IList<StateDto> _GetStatesByCountry(Guid countryGuid)
        {
            return Get<IList<StateDto>>("profile/GetStatesByCountry/" + countryGuid.ToString(), false);
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
  
        private WozCourseProgress _GetCurrentCourseProgress(Guid SubscriberGuid, Guid EnrollmentGuid)
        {
            return Get<WozCourseProgress>("woz/CourseStatus/" + SubscriberGuid + "/" + EnrollmentGuid, false);
        }


        public CountryDto _GetSubscriberCountry(int StateId)
        {
            return Get<CountryDto>("subscriber/CountryFromState/" + StateId, false);
        }

        public StateDto _GetSubscriberState(int StateId)
        {
            return Get<StateDto>("subscriber/State/" + StateId, false);
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
    }
}
 


