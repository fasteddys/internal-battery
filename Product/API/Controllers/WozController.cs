using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Newtonsoft;
using System.Reflection;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UpDiddyLib.Helpers;
using AutoMapper.QueryableExtensions;
using UpDiddyApi.Workflow;
using Hangfire;
using UpDiddy.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    // TODO Use Authorize 
    // [Authorize]
    public class WozController : Controller
    {

        #region Class Members
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly ISysLog _syslog;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        //private readonly CCQueue _queue = null;
        private readonly string _apiBaseUri = String.Empty;
        private readonly string _accessToken = String.Empty;
        private WozTransactionLog _log = null;
        private IHttpClientFactory _httpClientFactory = null;
        #endregion

        #region Constructor
        public WozController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysLog syslog, IHttpClientFactory httpClientFactory)
        {

            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            //_queue = new CCQueue("ccmessagequeue", _queueConnection);      
            _apiBaseUri = _configuration["Woz:ApiUrl"];
            _accessToken = _configuration["WozAccessToken"];            
            _log = new WozTransactionLog();
            _syslog = syslog;
            _httpClientFactory = httpClientFactory;
        }
        #endregion
        
        #region Courses 
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + "courses");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();
            return Ok(ResponseJson);
        }
        
        // TODO Deprecate 
        [HttpGet]
        // TODO Authorize [Authorize]
        // TODO Cache to every 10 minutes 
        [Route("api/[controller]/CourseStatus/{SubscriberGuid}/{EnrollmentGuid}")]
        public async Task<IActionResult> CourseStatus(string SubscriberGuid, string EnrollmentGuid)
        {

            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            if (Enrollment == null)
                return NotFound();

            // Get the woz course enrollment 
            WozCourseEnrollment WozEnrollment = _db.WozCourseEnrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            if (WozEnrollment == null)
                return NotFound();


            // Get the woz course enrollment 
            Subscriber CourseSubscriber = _db.Subscriber
                 .Where(t => t.IsDeleted == 0 && t.SubscriberGuid.ToString() == SubscriberGuid)
                 .FirstOrDefault();

            if (CourseSubscriber == null)
                return NotFound();

            if (Enrollment.SubscriberId != CourseSubscriber.SubscriberId)
                return Unauthorized();

            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + $"sections/{WozEnrollment.SectionId}/enrollments/{WozEnrollment.WozEnrollmentId}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var WozO = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                string LetterGrade = WozO.progress.letterGrade;
                string PercentageGrade = WozO.progress.percentageGrade;
                string ActivitiesCompleted = WozO.progress.activitiesCompleted;
                string ActivitiesTotal = WozO.progress.activitiesTotal;
                int _StatusCode = (int)response.StatusCode;
                WozCourseProgress CourseProgress = new WozCourseProgress()
                {
                    LetterGrade = LetterGrade,
                    PercentageGrade = int.Parse(PercentageGrade),
                    ActivitiesCompleted = int.Parse(ActivitiesCompleted),
                    ActivitiesTotal = int.Parse(ActivitiesTotal),
                    StatusCode = _StatusCode
                };
                return Ok(CourseProgress);
            }
            else
            {
                int _StatusCode = (int)response.StatusCode;
                WozCourseProgress wcp = new WozCourseProgress
                {
                    StatusCode = _StatusCode
                };
                return Ok(wcp);
            }
        }

 
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/UpdateStudentCourseStatus/{SubscriberGuid}/{FutureSchedule}")]
        public IActionResult UpdateStudentCourseStatus(string SubscriberGuid, bool FutureSchedule)
        {

            int AgeThresholdInHours = 6;
            try
            {
                AgeThresholdInHours = int.Parse(_configuration["ProgressUpdateAgeThresholdInHours"]);
            }
            catch { }

            BackgroundJob.Enqueue<ScheduledJobs>(j => j.UpdateStudentProgress(SubscriberGuid, AgeThresholdInHours)) ;
       
            // Queue another update in 6 hours 
            if ( FutureSchedule )
                BackgroundJob.Schedule<ScheduledJobs>(j => j.UpdateStudentProgress(SubscriberGuid, AgeThresholdInHours) ,TimeSpan.FromHours(AgeThresholdInHours)  );
             
            return Ok();
        }



        #endregion

        #region Students



        [HttpGet]
        // TODO Authorize [Authorize]
        [Route("api/[controller]/StudentInfo/{ExeterId}")]
        public async Task<IActionResult> StudentInfo(int ExeterId )
        {



            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + $"users/{ExeterId}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var WozO = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                // Must assign to local variables due to Newtonsoft 
                int _StatusCode = (int)response.StatusCode;
                long loginTimestamp = -1;
                DateTime? LastLogin = null;
                if (WozO.lastLoginDateUTC != null)
                {
                    loginTimestamp = (long)WozO.lastLoginDateUTC;
                    LastLogin = Utils.FromWozTime(loginTimestamp);
                }

                WozStudentInfoDto StudentInfo = new WozStudentInfoDto()
                {
                    ExeterId = ExeterId,
                    FirstName = (string)WozO.firstName,
                    LastName = (string)WozO.lastName,
                    EmailAddress = (string)WozO.emailAddress,
                    Address1 = (string)WozO.address1,
                    Address2 = (string)WozO.address2,
                    City = (string)WozO.city,
                    State = (string)WozO.state,
                    Country = (string)WozO.country,
                    PostalCode = (string)WozO.postalcode,
                    PhoneNumberPrimary = (string)WozO.phoneNumberPrimary,
                    PhoneNumberSecondary = (string)WozO.phoneNumberSeconday,
                    LastLoginDateUTCTimeStamp = loginTimestamp,
                    LastLoginDate = LastLogin
                };

          
                return Ok(StudentInfo);
            }
            else
            {
                int _StatusCode = (int)response.StatusCode;
                WozCourseProgress wcp = new WozCourseProgress
                {
                    StatusCode = _StatusCode
                };
                return Ok(wcp);
            }
 
        }


        #endregion

        #region Enrollments

        [HttpPut]
        [Authorize]
        [Route("api/[controller]/CancelEnrollment/{EnrollmentGuid}")]
        public async Task<IActionResult> CancelEnrollment(string EnrollmentGuid )
        { 
            // Get the Enrollment Object 
            WozCourseEnrollment WozEnrollment = _db.WozCourseEnrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            if (WozEnrollment == null || WozEnrollment.IsDeleted == 1)
                return NotFound();

            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            if (Enrollment == null || Enrollment.IsDeleted == 1)
                return NotFound();

            string action = $"sections/{WozEnrollment.SectionId}/enrollments/{WozEnrollment.WozEnrollmentId}";
            WozUpdateEnrollmentDto updateEnrollmentDto = new WozUpdateEnrollmentDto()
            {
                enrollmentStatus = (int) WozEnrollmentStatus.Canceled,
                enrollmentDateUTC = WozEnrollment.EnrollmentDateUTC,
                removalDateUTC = Utils.CurrentTimeInUnixMilliseconds()
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(updateEnrollmentDto);
            HttpClient client = WozPutClient();
            HttpRequestMessage request = WozPutRequest(action, json);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // Mark both records as deleted 
                WozEnrollment.IsDeleted = 1;
                WozEnrollment.ModifyDate = DateTime.Now;
                Enrollment.ModifyDate = DateTime.Now;
                Enrollment.IsDeleted = 1;
                _db.SaveChanges();
                _syslog.SysInfo($"WozController:CancelEnrollment: Woz response for enrollment {EnrollmentGuid} = {ResponseJson}");
                return Ok();                
            }
            else
            {
                _syslog.SysError($"WozController:CancelEnrollment: Woz response status code = {response.StatusCode.ToString()} for enrollment {EnrollmentGuid}" );
                return BadRequest();
            }          
        }

        #endregion



        #region Terms Of Service
        [HttpGet]
        [Route("api/[controller]/TermsOfService")]
        // TODO enable caching 
        // [ResponseCache(Duration = 600)]
        // Create or retreive a section for the the given course 
        public async Task<WozTermsOfServiceDto> TermsOfService()
        {
            
            WozTermsOfServiceDto RVal = new WozTermsOfServiceDto();

            try
            {                
                HttpClient client = WozGetClient();
                HttpRequestMessage WozRequest = WozGetRequest("tos");
                HttpResponseMessage WozResponse = await client.SendAsync(WozRequest);
                var ResponseJson = await WozResponse.Content.ReadAsStringAsync();

                if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {                 
                    var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);

                    string TermsOfServiceId = ResponseObject.termsOfServiceDocumentId;
                    string Content = ResponseObject.termsOfServiceContent;
                    RVal.DocumentId = int.Parse(TermsOfServiceId);
                    RVal.TermsOfService = Utils.RemoveRedundantSpaces(Utils.RemoveNewlines(Utils.RemoveHTML(Content)));
                  

                    // See if the latest TOS from woz has been stored to our local DB
                    WozTermsOfService tos = _db.WozTermsOfService
                        .Where(t => t.IsDeleted == 0 && t.DocumentId == RVal.DocumentId)                
                        .FirstOrDefault();

                    // Add the latest version to our database if it's not there 
                    if ( tos == null )
                    {
                        WozTermsOfService NewTermsOfService = _mapper.Map<WozTermsOfService>(RVal);
                        _db.WozTermsOfService.Add(NewTermsOfService);
                        _db.SaveChanges();
                    }

                }
                else
                    RVal = LastGoodTermsOfService();
            
            }
            catch( Exception ex )
            {
                RVal = LastGoodTermsOfService();
            }
            return RVal;
        }

            #endregion

        #region Helper Functions


         private WozTermsOfServiceDto LastGoodTermsOfService()
         {
            WozTermsOfServiceDto RVal = null;
            WozTermsOfService tos = _db.WozTermsOfService
                .Where(t => t.IsDeleted == 0)
                .OrderByDescending(t => t.DocumentId)
                .FirstOrDefault();

            if (tos != null)
                RVal = _mapper.Map<WozTermsOfServiceDto>(tos);
                
           return RVal;            
         }


        private HttpRequestMessage WozPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
               Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }


        private HttpRequestMessage WozPutRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }



        private HttpRequestMessage WozGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);                   
            return request;

        }


        private HttpClient WozGetClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;
        }


            private HttpClient WozPutClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPutClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private MessageTransactionResponse CreateResponse( string ResponseJson, string Info, string Data, TransactionState State)
        {
            MessageTransactionResponse RVal =  new MessageTransactionResponse()
            {
                ResponseJson = ResponseJson,
                InformationalMessage = Info,
                Data = Data,
                State = State
            };

            string RValJson = Newtonsoft.Json.JsonConvert.SerializeObject(RVal);
            return RVal;
        }
        #endregion

    }
}
