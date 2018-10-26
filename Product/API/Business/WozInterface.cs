using Android.Content;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Business
{
    public class WozInterface : BusinessVendorBase
    {

        #region Class
    

        public WozInterface(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["WozApiUrl"];
            _accessToken = configuration["WozAccessToken"];
        }

        #endregion

        #region Enroll Student
        public MessageTransactionResponse EnrollStudent(string EnrollmentGuid)
        {
            _translog = new WozTransactionLog();
            try
            {
                _translog.EndPoint = "Woz:EnrollStudent";
                _translog.InputParameters = $"enrollmentGuid={EnrollmentGuid}";

                // Get the Enrollment Object 
                Enrollment Enrollment = _db.Enrollment
                     .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                     .FirstOrDefault();

                // Check the validity of the request 
                if (Enrollment == null)
                    return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);

                _translog.EnrollmentGuid = Enrollment.EnrollmentGuid;

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
                    return CreateResponse(string.Empty, "Student login found", StudentLogin.VendorLogin, TransactionState.Complete);

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
                HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));
           
                string ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());

                _translog.WozResponseJson = ResponseJson;
                if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
                { 
                    var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                    try
                    {
                        string TransactionId = ResponseObject.transactionId;
                        string Message = "WozResponse = |" + ResponseObject.message + "| WozStudentDto = " + Json + "|";
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
            catch ( Exception ex )
            {                
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);

            }
        }

        public void ParseWozEnrollmentResource(string WozTransactionResponse, ref string ExeterId, ref string RegistrationUrl)
        {
            JObject WozJson = JObject.Parse(WozTransactionResponse);
            string WozResourceStr = (string)WozJson["resource"];
            var WozResourceObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(WozResourceStr);
            ExeterId = WozResourceObject.exeterId;
            RegistrationUrl = WozResourceObject.registrationUrl;
        }

        public MessageTransactionResponse CreateWozStudentLogin(VendorStudentLoginDto StudentLoginDto, string EnrollmentGuid)
        {
            _translog = new WozTransactionLog();
            _translog.EndPoint = "WozInterface:SaveWozStudentLogin";
            string WozEnrollmentJson = Newtonsoft.Json.JsonConvert.SerializeObject(StudentLoginDto);
            _translog.InputParameters = "WozEnrollmentJson=" + WozEnrollmentJson + ";";

            MessageTransactionResponse Rval = new MessageTransactionResponse();
            try
            {
                // Get the Enrollment Object 
                Enrollment Enrollment = _db.Enrollment
                     .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                     .FirstOrDefault();

                // Check the validity of the request 
                if (Enrollment == null)
                    return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);


                // Check to see if we need to enroll the student with the vendor 
                VendorStudentLogin ExistingStudentLogin = _db.VendorStudentLogin
                     .Where(v => v.IsDeleted == 0 &&
                                 v.SubscriberId == Enrollment.SubscriberId &&
                                 v.VendorId == Enrollment.Course.VendorId)
                     .FirstOrDefault();

                // Return user's login for the vendor 
                if (ExistingStudentLogin != null)
                    CreateResponse(string.Empty, "Student login found", ExistingStudentLogin.VendorLogin, TransactionState.Complete);


                VendorStudentLogin StudentLogin = _mapper.Map<VendorStudentLogin>(StudentLoginDto);
                StudentLogin.SubscriberId = Enrollment.SubscriberId;

                _db.VendorStudentLogin.Add(StudentLogin);
                _db.SaveChanges();
                CreateResponse(string.Empty, "Woz student login created", StudentLogin.VendorStudentLoginId.ToString(), TransactionState.Complete);
            }
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message + "Inner Exception: " + ex.InnerException.Message, string.Empty, TransactionState.FatalError);
            }
            return Rval;
        }


        #endregion

        #region Transaction Statuses 

        public MessageTransactionResponse TransactionStatus(string EnrollmentGuid, string TransactionId)
        {
            _translog = new WozTransactionLog();
            try
            {
                _translog.EndPoint = "WozInterface:TransactionStatus";
                _translog.InputParameters = "EnrollmentGuid=" + EnrollmentGuid + ";";
                _translog.InputParameters = "TransactionId=" + TransactionId + ";";

                try
                {
                    _translog.EnrollmentGuid = Guid.Parse(EnrollmentGuid);
                }
                catch (Exception ex)
                {
                    return CreateResponse(string.Empty, ex.Message, EnrollmentGuid, TransactionState.FatalError);
                }

                MessageTransactionResponse Rval = new MessageTransactionResponse();
                HttpClient client = WozClient();
                HttpRequestMessage WozRequest = WozGetRequest("transactions/" + TransactionId);
                HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));
                string ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());

                _translog.WozResponseJson = ResponseJson;
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
                        return CreateResponse(ResponseJson, "Transaction Complete", TransactionStatus, TransactionState.Complete);
                    }
                    catch (Exception ex)
                    {
                        return CreateResponse(ResponseJson, ex.Message, string.Empty, TransactionState.FatalError);
                    }
                }
                else if (WozResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return CreateResponse(ResponseJson, "404 Error", string.Empty, TransactionState.Error);
                }
                else
                {
                    return CreateResponse(ResponseJson, WozResponse.StatusCode.ToString() + " Error", string.Empty, TransactionState.Error);
                }


            }
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);
            }
        }



        #endregion  

        #region Class Sections

        //  Get a section for the specified Enrollment by either 1) Confirming that a section has already been created for the given
        //  enrollment or 2) Request Woz create a new section for the  enrollment 
        public MessageTransactionResponse GetSectionForEnrollment(string EnrollmentGuid)
        {
            _translog = new WozTransactionLog();

            try
            {
                _translog.EndPoint = "WozInterface:CreateSection";
                _translog.InputParameters = "EnrollmentGuid=" + EnrollmentGuid + ";";
                _translog.EnrollmentGuid = Guid.Parse(EnrollmentGuid);

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

                // Request a new section be created that is open for the current month
                DateTime FirstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                DateTime FirstDayOfMonthUtc = DateTime.SpecifyKind(FirstDayOfMonth, DateTimeKind.Utc);
                DateTime LastDayOfMonth = FirstDayOfMonth.AddMonths(1);
                DateTime LastDayOfMonthUtc = DateTime.SpecifyKind(LastDayOfMonth, DateTimeKind.Utc);
                long startDateUTC = ((DateTimeOffset)FirstDayOfMonthUtc).ToUnixTimeMilliseconds();
                long endDateUTC = ((DateTimeOffset)LastDayOfMonthUtc).ToUnixTimeMilliseconds();

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
                HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));

                string ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());
                
                _translog.WozResponseJson = ResponseJson;

                if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                    try
                    {
                        string TransactionId = ResponseObject.transactionId;
                        string Message = "WozResponse = |" + ResponseObject.message + "| WozSectionDto = " + Json + "|";
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
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);
            }
        }

        public MessageTransactionResponse SaveCourseSection(WozCourseSectionDto CourseSectionDto, string EnrollmentGuid)
        {

            _translog = new WozTransactionLog();

            try
            {
                _translog.EndPoint = "WozInterface:SaveCourseSection";
                string StudentLoginJson = Newtonsoft.Json.JsonConvert.SerializeObject(CourseSectionDto);
                _translog.InputParameters = "WozCourseSectionDto=" + StudentLoginJson + ";";
                _translog.EnrollmentGuid = Guid.Parse(EnrollmentGuid);

                MessageTransactionResponse Rval = new MessageTransactionResponse();
                try
                {
                    WozCourseSection CourseSection = _mapper.Map<WozCourseSection>(CourseSectionDto);
                    _db.WozCourseSection.Add(CourseSection);
                    _db.SaveChanges();
                    CreateResponse(string.Empty, "Course section login created", CourseSection.WozCourseSectionId.ToString(), TransactionState.Complete);
                }
                catch (Exception ex)
                {
                    return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);
                }

                return Rval;


            }
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);

            }
        }

        public WozCourseSectionDto ParseWozSectionResource(string WozTransactionResponse)
        {
            JObject WozJson = JObject.Parse(WozTransactionResponse);
            string WozResourceStr = (string)WozJson["resource"];
            var WozResourceObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(WozResourceStr);
            string CourseStartDateTimeStamp = WozResourceObject.startDateUTC;
            int Section = int.Parse((string)WozResourceObject.sectionId);
            string CourseCode = WozResourceObject.courseCode;
            // Convert unix time to c# time 
            long Timestamp = long.Parse(CourseStartDateTimeStamp);
            DateTime CourseStartDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            CourseStartDate = CourseStartDate.AddMilliseconds(Timestamp);
            // Get the section month and year 
            int Month = CourseStartDate.Month;
            int Year = CourseStartDate.Year;

            WozCourseSectionDto SectionDto = new WozCourseSectionDto()
            {
                WozCourseSectionId = 0,
                CourseCode = CourseCode,
                Section = Section,
                Month = Month,
                Year = Year,
                ModifyDate = DateTime.Now,
                CreateDate = DateTime.Now,
                IsDeleted = 0,
                CreateGuid = Guid.NewGuid()

            };
            return SectionDto;
        }



        #endregion


        #region Course Enrollment



        public MessageTransactionResponse SaveWozCourseEnrollment(string EnrollmentGuid, WozCourseEnrollmentDto WozCourseEnrollmentDto)
        {
            _translog = new WozTransactionLog();
            _translog.EndPoint = "WozInterface:SaveWozCourseEnrollment";
            string WozEnrollmentJson = Newtonsoft.Json.JsonConvert.SerializeObject(WozCourseEnrollmentDto);
            _translog.InputParameters = "WozEnrollmentJson=" + WozEnrollmentJson + ";";
            _translog.EnrollmentGuid = Guid.Parse(EnrollmentGuid);

            MessageTransactionResponse Rval = new MessageTransactionResponse();
            try
            {
                // Get the Enrollment Object 
                Enrollment Enrollment = _db.Enrollment
                     .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                     .FirstOrDefault();

                // Check the validity of the request 
                if (Enrollment == null)
                    return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);

                WozCourseEnrollment WozEnrollment = _mapper.Map<WozCourseEnrollment>(WozCourseEnrollmentDto);
              //  WozEnrollment.EnrollmentId = Enrollment.EnrollmentId;

                _db.WozCourseEnrollment.Add(WozEnrollment);
                _db.SaveChanges();
                CreateResponse(string.Empty, "Wox enrollment record created", WozEnrollment.WozCourseEnrollmentId.ToString(), TransactionState.Complete);

            }
            catch (Exception ex)
            {
                return CreateResponse(string.Empty, ex.Message, string.Empty, TransactionState.FatalError);
            }

            return Rval;
        }


 
        // Enroll a student with a vendor 
        public  MessageTransactionResponse RegisterStudent(string EnrollmentGuid)
        {
            _translog = new WozTransactionLog();
            _translog.EndPoint = "WozInterface:RegisterStudent";
            _translog.InputParameters = "EnrollmentGuid=" + EnrollmentGuid + ";";

            MessageTransactionResponse Rval = new MessageTransactionResponse();

            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            // Check the validity of the request 
            if (Enrollment == null)
                return CreateResponse(string.Empty, $"Enrollment {EnrollmentGuid} was not found.", EnrollmentGuid, TransactionState.FatalError);
            _translog.EnrollmentGuid = Enrollment.EnrollmentGuid;

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
            long EnrollmentDateUtc = ((DateTimeOffset)CurrentDateUtc).ToUnixTimeMilliseconds();
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


            // Check to see if the user is alreay enrolled
            WozCourseEnrollment StudentEnrollment = _db.WozCourseEnrollment
                .Where(v => v.IsDeleted == 0 &&
                            v.SectionId == Section.Section &&                            
                            v.ExeterId == ExeterId)
                .FirstOrDefault();

            if (StudentEnrollment != null)
                return CreateResponse(string.Empty, $"Exeter student {ExeterId} is already enrolled in section {Section.Section}", string.Empty, TransactionState.Complete);




            // Call Woz to enroll the student
            WozEnrollmentDto Student = new WozEnrollmentDto()
            {
                exeterId = ExeterId,
                enrollmentDateUTC = EnrollmentDateUtc
            };

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(Student);
 
            HttpClient client = WozClient();
            HttpRequestMessage WozRequest = WozPostRequest("sections/" + Section.Section.ToString() + "/enrollments", Json);
            HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));

            string ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());
            _translog.WozResponseJson = ResponseJson;

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


        public WozCourseEnrollmentDto ParseWozCourseEnrollmentResource(string EnrollmentGuid, string WozTransactionResponse)
        {
            JObject WozJson = JObject.Parse(WozTransactionResponse);
            string WozResourceStr = (string)WozJson["resource"];
            var WozResourceObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(WozResourceStr);
            int EnrollmentId = int.Parse((string)WozResourceObject.enrollmentId);
            int Section = int.Parse((string)WozResourceObject.sectionId);
            int EnrollmentStatus = int.Parse((string)WozResourceObject.enrollmentStatus);
            int ExeterId = int.Parse((string)WozResourceObject.exeterId);
            long CourseEnrollmentTimeStamp = long.Parse((string)WozResourceObject.enrollmentDateUTC);
            string CourseCode = WozResourceObject.CourseCode;
            DateTime CourseStartDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            CourseStartDate = CourseStartDate.AddMilliseconds(CourseEnrollmentTimeStamp);

            WozCourseEnrollmentDto EnrollmentDto = new WozCourseEnrollmentDto()
            {
                WozCourseEnrollmentId = 0,
                WozEnrollmentId = EnrollmentId,
                SectionId = Section,
                EnrollmentStatus = EnrollmentStatus,
                ExeterId = ExeterId,
                EnrollmentDateUTC = CourseEnrollmentTimeStamp,
                EnrollmentGuid = Guid.Parse(EnrollmentGuid),
                ModifyDate = DateTime.Now,
                CreateDate = DateTime.Now,
                IsDeleted = 0,
                CreateGuid = Guid.NewGuid()

            };
            return EnrollmentDto;
        }

        #endregion

        #region Utility Functions


  




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

        private MessageTransactionResponse CreateResponse(string ResponseJson, string Info, string Data, TransactionState State)
        {
            MessageTransactionResponse RVal = new MessageTransactionResponse()
            {
                ResponseJson = ResponseJson,
                InformationalMessage = Info,
                Data = Data,
                State = State
            };

            string RValJson = Newtonsoft.Json.JsonConvert.SerializeObject(RVal);

            if (_translog.WozTransactionLogId > 0) _translog.WozTransactionLogId = 0;
            _translog.ResponseJson = RValJson;
            _translog.ModifyDate = DateTime.Now;
            _translog.CreateDate = DateTime.Now;
            _translog.CreateGuid = Guid.NewGuid();
            _translog.ModifyGuid = Guid.NewGuid();

            _db.WozTransactionLog.Add(_translog);
            _db.SaveChanges();

            return RVal;
        }


   


    }

    #endregion

        #region async helper

    internal static class xAsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return xAsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            xAsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }

    #endregion


}
