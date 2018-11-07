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
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        //private readonly CCQueue _queue = null;
        private readonly string _apiBaseUri = String.Empty;
        private readonly string _accessToken = String.Empty;
        private WozTransactionLog _log = null;
        #endregion

        #region Constructor
        public WozController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {

            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            //_queue = new CCQueue("ccmessagequeue", _queueConnection);      
            _apiBaseUri = _configuration["Woz:ApiUrl"];
            _accessToken = _configuration["WozAccessToken"];            
            _log = new WozTransactionLog();
        }
        #endregion

         

        #region Courses 
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + "courses");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();
            return Ok(ResponseJson);

        }





        [HttpGet]
        // TODO Authorize [Authorize]
        // TODO Cache to every 10 minutes 
        [Route("api/[controller]/CourseSchedule/{CourseCode}/{CourseGuid}")]
        public async Task<IActionResult> CourseSchedule(string CourseCode, Guid CourseGuid)
        {

            int MonthsLookAhead = 6;
            if (!(int.TryParse(_configuration["Woz:CourseScheduleMonthLookahead"], out MonthsLookAhead)))
                MonthsLookAhead = 6;
            

            long UTCStartDate = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
            long UTCEndDate = ((DateTimeOffset)DateTime.Now.AddMonths(MonthsLookAhead)).ToUnixTimeMilliseconds();
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + $"courses/{CourseCode}/schedule?startDateUTC={UTCStartDate.ToString()}&endDateUTC={UTCEndDate.ToString()}");
 
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();

            IList<CourseVariantDto> Variants = null;
            /*todo: fix after getting through migration issues
             * 
             * Variants = _db.CourseVariant
                .Join(_db.Course,
                cv => cv.CourseId,
                c => c.CourseId)
                .Where(t => t.IsDeleted == 0 && t.CourseGuid == CourseGuid)
                .ProjectTo<CourseVariantDto>(_mapper.ConfigurationProvider)
                .ToList();
                */
            List<Tuple<int, string, Decimal>> VarToPrice = new List<Tuple<int, string, decimal>>();
            foreach (CourseVariantDto variant in Variants)
            {
                VarToPrice.Add(new Tuple<int, string, Decimal>(variant.CourseVariantId, variant.VariantType, variant.Price));
            }

            WozCourseScheduleDto CourseSchedule = new WozCourseScheduleDto();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var WozO = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                JArray StartDatesJsonArray = WozO.startDatesUTC;

                IList<long> CourseStartDatesUtc = null;
                try
                {
                    CourseStartDatesUtc = StartDatesJsonArray.Select(jv => (long)jv).ToList();
                }
                catch{}


                CourseSchedule.CourseCode = CourseCode;
                CourseSchedule.StartDatesUTC = CourseStartDatesUtc;
                CourseSchedule.VariantToPrice = VarToPrice;

                return Ok(CourseSchedule);
            }
            else
            {
                CourseSchedule.CourseCode = string.Empty;
                CourseSchedule.StartDatesUTC = null;
                CourseSchedule.VariantToPrice = VarToPrice;
                return Ok(CourseSchedule);
            }

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

            HttpClient client = new HttpClient();
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
        [Route("api/[controller]/UpdateStudentCourseStatus/{SubscriberGuid}")]
        public IActionResult UpdateStudentCourseStatus(string SubscriberGuid)
        {             
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.UpdateStudentProgress(SubscriberGuid)) ;      
            return Ok();
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
                HttpClient client = WozClient();
                HttpRequestMessage WozRequest = WozGetRequest("tos");
                HttpResponseMessage WozResponse = await client.SendAsync(WozRequest);
                var ResponseJson = await WozResponse.Content.ReadAsStringAsync();

                if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // dynamic ResponseObject  = System.Web.Helpers.Json.Decode(ResponseJson);
                    //dynamic ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
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

        private HttpRequestMessage WozGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);                   
            return request;

        }




        private HttpClient WozClient()
        {
            HttpClient client = new HttpClient();
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
