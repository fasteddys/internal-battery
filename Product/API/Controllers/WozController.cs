using System;
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
using UpDiddyLib.Helpers;
using UpDiddyApi.Workflow;
using Hangfire;
using UpDiddy.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    // TODO Use Authorize
    // todo: should this be a service? vs a controller?
    // [Authorize]
    public class WozController : Controller
    {

        #region Class Members
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly ILogger _syslog;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly string _apiBaseUri = String.Empty;
        private readonly string _accessToken = String.Empty;
        private WozTransactionLog _log = null;
        private IHttpClientFactory _httpClientFactory = null;
        #endregion

        #region Constructor
        public WozController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<WozController> syslog, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _apiBaseUri = _configuration["Woz:ApiUrl"];
            _accessToken = _configuration["Woz:AccessToken"];
            _log = new WozTransactionLog();
            _syslog = syslog;
            _httpClientFactory = httpClientFactory;
        }
        #endregion
        
        #region Courses
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/update-student-course-status/{FutureSchedule}")]
        public IActionResult UpdateStudentCourseStatus(bool futureSchedule)
        {
            string subscriberGuid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var NumEnrollments = _db.Enrollment
                 .Include(s => s.Subscriber)
                 .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid.ToString() == subscriberGuid && s.CompletionDate == null && s.DroppedDate == null)
                .Count();
            // Short circuit if the user does not have any enrollments 
            if (NumEnrollments == 0)
                return Ok();
     
            int AgeThresholdInHours = int.Parse(_configuration["ProgressUpdateAgeThresholdInHours"]);
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.UpdateStudentProgress(subscriberGuid, AgeThresholdInHours)) ;
       
            // Queue another update in 6 hours 
            if (futureSchedule)
                BackgroundJob.Schedule<ScheduledJobs>(j => j.UpdateStudentProgress(subscriberGuid, AgeThresholdInHours) ,TimeSpan.FromHours(AgeThresholdInHours)  );
             
            return Ok();
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
