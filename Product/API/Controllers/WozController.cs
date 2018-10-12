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
        private readonly CCQueue _queue = null;
        private  readonly string _apiBaseUri = String.Empty;
        private readonly string _accessToken = String.Empty;
        private readonly WozTransactionLog _log = null;
        #endregion

        #region Constructor
        public WozController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _queue = new CCQueue("ccmessagequeue", _queueConnection);
            // TODO put in configuration
          //  _apiBaseUri = "https://clientapi.qa.exeterlms.com/v1/";
            _apiBaseUri = _configuration["Woz:ApiUrl"];
            _accessToken = _configuration["WozAccessToken"];
            _log = new WozTransactionLog();

        }
        #endregion

        #region Student Enrollment 

        [HttpPost]
        [Route("api/[controller]/SaveWozEnrollment/{EnrollmentGuid}")]
        // Save student's vendor specific enrollment info to database  
        public MessageTransactionResponse SaveWozEnrollment([FromBody] WozCourseEnrollmentDto WozEnrollmentDto, [FromRoute] string EnrollmentGuid)
        {
            _log.EndPoint = "WozCourseEnrollmentDto";
            string WozEnrollmentJson = Newtonsoft.Json.JsonConvert.SerializeObject(WozEnrollmentDto);
            _log.InputParameters = "WozEnrollmentJson=" + WozEnrollmentJson + ";";
            _log.EnrollmentGuid = Guid.Parse(EnrollmentGuid);

            MessageTransactionResponse Rval = new MessageTransactionResponse();
            try
            {/*
                // Get the Enrollment Object 
                Enrollment Enrollment = _db.Enrollment
                     .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                     .FirstOrDefault();

                // Check the validity of the request 
                if (Enrollment == null)
                    return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);

                WozCourseEnrollment WozEnrollment = _mapper.Map<WozCourseEnrollment>(WozEnrollmentDto);
                WozEnrollment.EnrollmentId = Enrollment.EnrollmentId;

                _db.WozCourseEnrollment.Add(WozEnrollment);
                _db.SaveChanges();
                CreateResponse(string.Empty, "Wox enrollment record created", WozEnrollment.WozCourseEnrollmentId.ToString(), TransactionState.Complete);
                */
            }
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);
            }

            return Rval;
        }




        [HttpPost]
        [Route("api/[controller]/SaveStudentEnrollment/{EnrollmentGuid}")]
        // Save student's vendor enrollment info to database  
        public MessageTransactionResponse SaveStudentEnrollment([FromBody] VendorStudentLogin StudentLogin, [FromRoute] string EnrollmentGuid)
        {
            _log.EndPoint = "SaveStudentEnrollment";
            string StudentLoginJson = Newtonsoft.Json.JsonConvert.SerializeObject(StudentLogin);
            _log.InputParameters = "VendorStudentLogin=" + StudentLoginJson + ";";
            _log.EnrollmentGuid = Guid.Parse(EnrollmentGuid);

            MessageTransactionResponse Rval = new MessageTransactionResponse();

            try
            {
                _db.VendorStudentLogin.Add(StudentLogin);
                _db.SaveChanges();
                CreateResponse(string.Empty, "Student enrollment created", StudentLogin.VendorStudentLoginId.ToString(), TransactionState.Complete);

            }
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);
            }

            return Rval;
        }



        [HttpGet]
        [Route("api/[controller]/EnrollStudent/{EnrollmentGuid}")]
        // Enroll a student with a vendor 
        public async Task<MessageTransactionResponse> EnrollStudent(string EnrollmentGuid)
        {

            _log.EndPoint = "EnrollStudent";
            _log.InputParameters = "EnrollmentGuid=" + EnrollmentGuid + ";";

            MessageTransactionResponse Rval = new MessageTransactionResponse();

            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            // Check the validity of the request 
            if (Enrollment == null)
                return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);
            _log.EnrollmentGuid = Enrollment.EnrollmentGuid;

            // Validate we have a subscriber
            _db.Entry(Enrollment).Reference(s => s.Subscriber).Load();
            if (Enrollment.Subscriber == null)
                return CreateResponse(string.Empty, $"Subscriber with id {Enrollment.SubscriberId} was not found.", Enrollment.SubscriberId.ToString(), TransactionState.FatalError);

            // Valid we have a course                         
            _db.Entry(Enrollment).Reference(c => c.Course).Load();
            if (Enrollment.Course == null)
                return CreateResponse(string.Empty, $"Course with id {Enrollment.CourseId} was not found.", Enrollment.SubscriberId.ToString(), TransactionState.FatalError);

            // Check to see if we need to enroll the student with the vendor 
            VendorStudentLogin StudentLogin = _db.VendorStudentLogin
                 .Where(v => v.IsDeleted == 0 &&
                             v.SubscriberId == Enrollment.SubscriberId &&
                             v.VendorId == Enrollment.Course.VendorId)
                 .FirstOrDefault();

            // Return user's login for the vendor 
            if (StudentLogin != null)
                CreateResponse(string.Empty, "Student login found", StudentLogin.VendorLogin, TransactionState.Complete);

            // Call Woz to register student
            WozStudentDto Student = new WozStudentDto()
            {
                firstName = Enrollment.Subscriber.FirstName,
                lastName = Enrollment.Subscriber.LastName,
                emailAddress = Enrollment.Subscriber.Email,
                acceptedTermsOfServiceDocumentId = Enrollment.TermsOfServiceFlag == null ? 0 : (int)Enrollment.TermsOfServiceFlag,
                suppressRegistrationEmail = false
            };

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(Student);
            HttpClient client = WozClient();
            HttpRequestMessage WozRequest = WozPostRequest("users", Json);
            HttpResponseMessage WozResponse = await client.SendAsync(WozRequest);
            var ResponseJson = await WozResponse.Content.ReadAsStringAsync();
            _log.WozResponseJson = ResponseJson;

            if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // dynamic ResponseObject  = System.Web.Helpers.Json.Decode(ResponseJson);
                //dynamic ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                try
                {
                    string TransactionId = ResponseObject.transactionId;
                    string Message = ResponseObject.message;
                    return CreateResponse(ResponseJson, Message, TransactionId, TransactionState.InProgress);
                }
                catch (Exception ex)
                {
                    return CreateResponse(ResponseJson, ex.Message, string.Empty, TransactionState.FatalError);
                }
            }
            else
            {
                return CreateResponse(ResponseJson, WozResponse.StatusCode.ToString() + " Error", string.Empty, TransactionState.Error);
            }


        }


        #endregion

        #region Register Student to Course 
        [HttpGet]
        [Route("api/[controller]/RegisterStudent/{EnrollmentGuid}")]
        // Enroll a student with a vendor 
        public async Task<MessageTransactionResponse> RegisterStudent(string EnrollmentGuid)
        {

            _log.EndPoint = "RegisterStudent";
            _log.InputParameters = "EnrollmentGuid=" + EnrollmentGuid + ";";

            MessageTransactionResponse Rval = new MessageTransactionResponse();

            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            // Check the validity of the request 
            if (Enrollment == null)
                return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);
            _log.EnrollmentGuid = Enrollment.EnrollmentGuid;

            // Valid we have a course                         
            _db.Entry(Enrollment).Reference(c => c.Course).Load();
            if (Enrollment.Course == null)
                return CreateResponse(string.Empty, $"Course with id {Enrollment.CourseId} was not found.", Enrollment.SubscriberId.ToString(), TransactionState.FatalError);

            // Check to see if we need to enroll the student with the vendor 
            VendorStudentLogin StudentLogin = _db.VendorStudentLogin
                 .Where(v => v.IsDeleted == 0 &&
                             v.SubscriberId == Enrollment.SubscriberId &&
                             v.VendorId == Enrollment.Course.VendorId)
                 .FirstOrDefault();

            // We need to have a login for the student to proceed 
            if (StudentLogin == null)
                CreateResponse(string.Empty, "Student not login found", string.Empty, TransactionState.Error);

            int ExeterId = int.Parse(StudentLogin.VendorLogin);

            // Get section for the course 
            DateTime CurrentDate = DateTime.Now;
            DateTime CurrentDateUtc = DateTime.SpecifyKind(CurrentDate, DateTimeKind.Utc);
            long EnrollmentDateUtc = ((DateTimeOffset)CurrentDateUtc).ToUnixTimeSeconds();
            int Year = CurrentDate.Year;
            int Month = CurrentDate.Month;

            WozCourseSection Section = _db.WozCourseSection
                .Where(v => v.IsDeleted == 0 &&
                            v.CourseCode == Enrollment.Course.Code &&
                            v.Month == Month &&
                            v.Year == Year)
                .FirstOrDefault();

            // If a section has already been created, return the section ID to the caller 
            if (Section == null)
                return CreateResponse(string.Empty, "Section not found", Section.Section.ToString(), TransactionState.Error);

            // Call Woz to enroll the student
            WozEnrollmentDto Student = new WozEnrollmentDto()
            {
                exeterId = ExeterId,
                enrollmentDateUTC = EnrollmentDateUtc
            };

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(Student);
            HttpClient client = WozClient();
            HttpRequestMessage WozRequest = WozPostRequest("sections/" + Section.Section.ToString() + "/enrollments", Json);
            HttpResponseMessage WozResponse = await client.SendAsync(WozRequest);
            var ResponseJson = await WozResponse.Content.ReadAsStringAsync();
            _log.WozResponseJson = ResponseJson;

            if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                try
                {
                    string TransactionId = ResponseObject.transactionId;
                    string Message = ResponseObject.message;
                    return CreateResponse(ResponseJson, Message, TransactionId, TransactionState.InProgress);
                }
                catch (Exception ex)
                {
                    return CreateResponse(ResponseJson, ex.Message, string.Empty, TransactionState.FatalError);
                }
            }
            else
            {
                return CreateResponse(ResponseJson, WozResponse.StatusCode.ToString() + " Error", string.Empty, TransactionState.Error);
            }
        }

        #endregion

        #region Course Sections 

        [HttpPost]
        [Route("api/[controller]/SaveCourseSection/{EnrollmentGuid}")]
        // Save a section for database                         
        public MessageTransactionResponse SaveCourseSection([FromBody]  WozCourseSection CouseSection, [FromRoute] string EnrollmentGuid)
        {
            _log.EndPoint = "SaveSection";
            string StudentLoginJson = Newtonsoft.Json.JsonConvert.SerializeObject(CouseSection);
            _log.InputParameters = "WozCourseSection=" + StudentLoginJson + ";";
            _log.EnrollmentGuid = Guid.Parse(EnrollmentGuid);

            MessageTransactionResponse Rval = new MessageTransactionResponse();
            try
            {
                _db.WozCourseSection.Add(CouseSection);
                _db.SaveChanges();
                CreateResponse(string.Empty, "Course section login created", CouseSection.WozCourseSectionId.ToString(), TransactionState.Complete);

            }
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);
            }

            return Rval;
        }
      


        [HttpGet]
        [Route("api/[controller]/CreateSection/{EnrollmentGuid}")]
        // Create or retreive a section for the the given course 
        public async Task<MessageTransactionResponse> CreateSection(string EnrollmentGuid)
        {
            _log.EndPoint = "CreateSection";
            _log.InputParameters = "EnrollmentGuid=" + EnrollmentGuid + ";";
            _log.EnrollmentGuid = Guid.Parse(EnrollmentGuid);

            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            // Check the validity of the request 
            if (Enrollment == null)
                return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);

            // Valid we have a course                         
            _db.Entry(Enrollment).Reference(c => c.Course).Load();
            if (Enrollment.Course == null)
                return CreateResponse(string.Empty, $"Course with id {Enrollment.CourseId} was not found.", Enrollment.SubscriberId.ToString(), TransactionState.FatalError);

            string CourseCode = Enrollment.Course.Code;

            // See if the section for the course has been created for the current month and year 
            DateTime CurrentDate = DateTime.UtcNow;
            int Year = CurrentDate.Year;
            int Month = CurrentDate.Month;

            WozCourseSection Section = _db.WozCourseSection
                .Where(v => v.IsDeleted == 0 &&
                            v.CourseCode == CourseCode &&
                            v.Month == Month && 
                            v.Year == Year)
                .FirstOrDefault();

            // If a section has already been created, return the section ID to the caller 
            if (Section != null)
                return CreateResponse(string.Empty, string.Empty, Section.Section.ToString(), TransactionState.Complete);

            // Request a new section be created 
            
            DateTime FirstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            DateTime FirstDayOfMonthUtc = DateTime.SpecifyKind(FirstDayOfMonth, DateTimeKind.Utc);
            DateTime LastDayOfMonth = FirstDayOfMonth.AddMonths(1);
            DateTime LastDayOfMonthUtc = DateTime.SpecifyKind(LastDayOfMonth, DateTimeKind.Utc);
            long startDateUTC = ((DateTimeOffset)FirstDayOfMonthUtc).ToUnixTimeSeconds();
            long endDateUTC = ((DateTimeOffset)LastDayOfMonthUtc).ToUnixTimeSeconds();
 
            WozSectionDto NewSection = new WozSectionDto()
            {
                courseCode = CourseCode,
                startDateUTC = startDateUTC,
                endDateUTC = endDateUTC,
                isOpen = true,
                maxStudents = 0,
                timeZone = "Eastern Standard Time"
            };

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(NewSection);
            HttpClient client = WozClient();
            HttpRequestMessage WozRequest = WozPostRequest("sections", Json);
            HttpResponseMessage WozResponse = await client.SendAsync(WozRequest);
            var ResponseJson = await WozResponse.Content.ReadAsStringAsync();

            _log.WozResponseJson = ResponseJson;

            if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                 
                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                try
                {
                    string TransactionId = ResponseObject.transactionId;
                    string Message = ResponseObject.message;
                    return CreateResponse(ResponseJson, Message, TransactionId, TransactionState.InProgress);
                }
                catch (Exception ex)
                {
                    return CreateResponse(ResponseJson, ex.Message, string.Empty, TransactionState.FatalError);
                }
            }
            else
            {
                return CreateResponse(ResponseJson, WozResponse.StatusCode.ToString() + " Error", string.Empty, TransactionState.Error);
            }


        }

        #endregion

        #region Transactions 

        [HttpGet]
        [Route("api/[controller]/TransactionStatus/{EnrollmentGuid}/{TransactionId}")]
        // check the status of an enrollment transaction
        public async Task<MessageTransactionResponse> TransactionStatus(string EnrollmentGuid, string TransactionId)
        {
            _log.EndPoint = "TransactionStatus";
            _log.InputParameters = "EnrollmentGuid=" + EnrollmentGuid + ";";
            _log.InputParameters = "TransactionId=" + TransactionId + ";";

            try
            {
                _log.EnrollmentGuid = Guid.Parse(EnrollmentGuid);
            }
            catch(Exception ex )
            {
                return CreateResponse(string.Empty,ex.Message, EnrollmentGuid, TransactionState.FatalError);
            }
            

            MessageTransactionResponse Rval = new MessageTransactionResponse();

            HttpClient client = WozClient();
            HttpRequestMessage WozRequest = WozGetRequest("transactions/" + TransactionId);
            HttpResponseMessage WozResponse = await client.SendAsync(WozRequest);
            var ResponseJson = await WozResponse.Content.ReadAsStringAsync();
            _log.WozResponseJson = ResponseJson;
 

            if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
            
                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                try
                {
                    // Woz Statuses
                    //  Created = 0
                    //  Scheduled = 100
                    //  Staged = 200
                    //  Processing = 300
                    //  Succeeded = 400
                    //  Failed = 500
                    //  Cancelled = 600
                    //  Expired = 700
                    
                    string TransactionStatus = ResponseObject.status;
                    return CreateResponse(ResponseJson,"Transaction Complete", TransactionStatus, TransactionState.Complete);
                }
                catch (Exception ex)
                {
                    return CreateResponse(ResponseJson, ex.Message, string.Empty, TransactionState.FatalError);
                }
            }
            else if (WozResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return CreateResponse(ResponseJson, "404 Error",string.Empty, TransactionState.Error);
            }
            else
            {
                return CreateResponse(ResponseJson, WozResponse.StatusCode.ToString() + " Error", string.Empty, TransactionState.Error);
            }
            
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
                    RVal.TermsOfService = Content;
                     
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
            _log.ResponseJson = RValJson;
            _log.ModifyDate = DateTime.Now;
            _log.CreateDate = DateTime.Now;
            _log.CreateGuid = Guid.NewGuid();
            _log.ModifyGuid = Guid.NewGuid();

            _db.WozTransactionLog.Add(_log);
            _db.SaveChanges();

            return RVal;
        }
        #endregion

    }
}
