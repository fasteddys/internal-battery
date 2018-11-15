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
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;
using UpDiddy.Helpers;

namespace UpDiddyApi.Business
{
    public class WozInterface : BusinessVendorBase
    {

        #region Class


        public WozInterface(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysLog sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = context;
            _mapper = mapper;
            // TODO: CRITICAL, Azure Key Vault does NOT permit colons. See https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.1
            _apiBaseUri = configuration["Woz:ApiUrl"];
            _accessToken = configuration["Woz:AccessToken"];
            _syslog = sysLog;
            _configuration = configuration;
            _HttpClientFactory = httpClientFactory;
        }

        #endregion

        #region Enroll Student
        public MessageTransactionResponse EnrollStudent(string EnrollmentGuid, ref bool IsInstructorLed)
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

                // Determine if the course is instructor led
                if (Enrollment.EnrollmentStatusId == (int)EnrollmentStatus.FutureRegisterStudentRequested)
                    IsInstructorLed = true;

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
                WozCreateStudentDto Student = new WozCreateStudentDto()
                {
                    firstName = Enrollment.Subscriber.FirstName,
                    lastName = Enrollment.Subscriber.LastName,
                    emailAddress = Enrollment.Subscriber.Email,
                    phoneNumberPrimary = Enrollment.Subscriber.PhoneNumber,
                    acceptedTermsOfServiceDocumentId = Enrollment.TermsOfServiceFlag == null ? 0 : (int)Enrollment.TermsOfServiceFlag,
                    suppressRegistrationEmail = false
                };

                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(Student);
                string ResponseJson = string.Empty;
                HttpResponseMessage WozResponse = ExecuteWozPost("users", Json, ref ResponseJson);

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
            catch (Exception ex)
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
            _translog.EndPoint = "WozInterface:CreateWozStudentLogin";
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

                // Valid we have a course                         
                _db.Entry(Enrollment).Reference(c => c.Course).Load();

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

                string ResponseJson = string.Empty;
                HttpResponseMessage WozResponse = ExecuteWozGet("transactions/" + TransactionId, ref ResponseJson);

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
                string ResponseJson = string.Empty;
                HttpResponseMessage WozResponse = ExecuteWozPost("sections", Json, ref ResponseJson);

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
 
        public async Task<WozCourseProgress> GetCourseProgress(int SectionId, int WozEnrollmentId)
        {

            var Url = _apiBaseUri + $"sections/{SectionId}/enrollments/{WozEnrollmentId}";

            
            HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpGetClientName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Url);
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
                return CourseProgress;
            }
            else
            {
                _syslog.Log(LogLevel.Error,"WozInterface:GetCourseProgress Returned a status code of " + response.StatusCode.ToString());
                _syslog.Log(LogLevel.Error, "WozInterface:GetCourseProgress Url =  " + Url);
                _syslog.Log(LogLevel.Error, "WozInterface:GetCourseProgress AccessToken ends with  " + _accessToken.Substring( _accessToken.Length - 2));
                return null;
            }

        }


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



        public MessageTransactionResponse RegisterStudentInstructorLed(string EnrollmentGuid)
        {
            _translog = new WozTransactionLog();
            _translog.EndPoint = "WozInterface:RegisterStudentInstructorLed";
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

            WozFutureEnrollmentDto EnrollmentInfo = new WozFutureEnrollmentDto()
            {
                exeterId = ExeterId,
                sectionStartDateUTC = (long)Enrollment.SectionStartTimestamp,
                courseCode = Enrollment.Course.Code
            };

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(EnrollmentInfo);
            string ResponseJson = string.Empty;
            HttpResponseMessage WozResponse = ExecuteWozPost("/enrollments/future", Json, ref ResponseJson);
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

        public List<DateTime> CheckCourseSchedule(string courseCode)
        {
            List<DateTime> Rval = null;
            try
            {
                int MonthsLookAhead = 6;
                int.TryParse(_configuration["Woz:CourseScheduleMonthLookahead"], out MonthsLookAhead);
                // setting lower bound to be 2 days into the future to prevent scheduling issues for Woz
                long UTCStartDate = ((DateTimeOffset)DateTime.Now.Date.AddDays(2)).ToUnixTimeMilliseconds();
                long UTCEndDate = ((DateTimeOffset)DateTime.Now.AddMonths(MonthsLookAhead)).ToUnixTimeMilliseconds();
                HttpClient client = new HttpClient();
                string ResponseJson = string.Empty;
                ExecuteWozGet($"courses/{courseCode}/schedule?startDateUTC={UTCStartDate.ToString()}&endDateUTC={UTCEndDate.ToString()}", ref ResponseJson);
                JObject ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                Rval = ResponseObject
                    .Properties()
                    .Where(ro => ro.Name == "startDatesUTC")
                    .Select(ro => ro.Value)
                    .Values<long>()
                    .Select(x => FromWozTime(x))
                    .ToList();
            }
            catch (Exception e)
            {
            }
            return Rval;
        }

        // Enroll a student with a vendor 
        public MessageTransactionResponse RegisterStudent(string EnrollmentGuid)
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
            string ResponseJson = string.Empty;
            HttpResponseMessage WozResponse = ExecuteWozPost("sections/" + Section.Section.ToString() + "/enrollments", Json, ref ResponseJson);

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

        #region Terms of Service
        
        public WozTermsOfServiceDto GetTermsOfService()
        {
            WozTermsOfServiceDto Rval = new WozTermsOfServiceDto();
            try
            {
                HttpClient client = new HttpClient();
                string ResponseJson = string.Empty;
                ExecuteWozGet("tos", ref ResponseJson);

                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                Rval.DocumentId = ResponseObject.termsOfServiceDocumentId.ToObject<int>();
                Rval.WozTermsOfServiceId = ResponseObject.termsOfServiceDocumentId.ToObject<int>();
                Rval.TermsOfService = Utils.RemoveRedundantSpaces(Utils.RemoveNewlines(Utils.RemoveHTML(ResponseObject.termsOfServiceContent.ToObject<string>()))); 

                // See if the latest TOS from woz has been stored to our local DB
                WozTermsOfService tos = _db.WozTermsOfService
                    .Where(t => t.IsDeleted == 0 && t.DocumentId == Rval.DocumentId)
                    .FirstOrDefault();

                // Add the latest version to our database if it's not there 
                if (tos == null)
                {
                    WozTermsOfService NewTermsOfService = _mapper.Map<WozTermsOfService>(Rval);
                    _db.WozTermsOfService.Add(NewTermsOfService);
                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                // if something goes wrong, get the most recent TOS from our system
                WozTermsOfService tos = _db.WozTermsOfService
                    .Where(t => t.IsDeleted == 0)
                    .OrderByDescending(t => t.DocumentId)
                    .FirstOrDefault();

                if (tos != null)
                    Rval = _mapper.Map<WozTermsOfServiceDto>(tos);
            }
            return Rval;
        }


        #endregion

        #region Scheduled Tasks
        // TODO Add Logging 
        public bool ReconcileFutureEnrollment(string EnrollmentGuid)
        {
            try
            {
                _syslog.Log(LogLevel.Information,$"ReconcileFutureEnrollment: Starting with EnrollmentGuid =  {EnrollmentGuid}");
                // Get the Enrollment Object 
                Enrollment Enrollment = _db.Enrollment
                     .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                     .FirstOrDefault();

                if (Enrollment == null)
                {
                    _syslog.Log(LogLevel.Information, $"ReconcileFutureEnrollment: Enrollment {EnrollmentGuid} not found!");
                    return false;
                }

                // Short circuit if current date >= the Friday before the course is to begin
                DateTime StartDate = Utils.FromWozTime((long)Enrollment.SectionStartTimestamp);
                DateTime PriorFriday = Utils.PriorDayOfWeek(StartDate, System.DayOfWeek.Friday);

                if (PriorFriday > DateTime.Now)
                {
                    _syslog.Log(LogLevel.Information, $"ReconcileFutureEnrollment:  Too early to check for enrollment.  PriorFriday = {PriorFriday.ToLongDateString() } ");
                    return false;
                }



                // Load the asscociated course     
                _db.Entry(Enrollment).Reference(c => c.Course).Load();
                // Confirm that the enrollment has a status of future 
                if (Enrollment.EnrollmentStatusId != (int) EnrollmentStatus.FutureRegisterStudentComplete)
                {                   
                        _syslog.Log(LogLevel.Information, $"ReconcileFutureEnrollment: Enrollment {EnrollmentGuid} is not a FutureRegisterStudentComplete. EnrollmentStatus = {Enrollment.EnrollmentStatusId} ");
                        return false;
                }


                // Check to see if we need to enroll the student with the vendor 
                VendorStudentLogin StudentLogin = _db.VendorStudentLogin
                     .Where(v => v.IsDeleted == 0 &&
                                 v.SubscriberId == Enrollment.SubscriberId &&
                                 v.VendorId == Enrollment.Course.VendorId)
                     .FirstOrDefault();

                if (StudentLogin == null)
                {
                    _syslog.Log(LogLevel.Information, $"ReconcileFutureEnrollment: Unable to locate VendorStudentLogin for enrollment {EnrollmentGuid}. SubscriberId = {Enrollment.SubscriberId} VendorId = {Enrollment.Course.VendorId} ");
                    return false;
                }


                long UTCNowUnixMilliseconds = Utils.CurrentTimeInUnixMilliseconds();
                WozActiveOfRequestDto ActiveOf = new WozActiveOfRequestDto()
                {
                    activeAsOfDateUTC = UTCNowUnixMilliseconds
                };


                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(ActiveOf);
                _syslog.Log(LogLevel.Information,$"ReconcileFutureEnrollment: WozActiveOfRequest =  {Json} ");
                string ResponseJson = string.Empty;
                HttpResponseMessage Response  = ExecuteWozPost($"/users/{StudentLogin.VendorLogin}/enrollments", Json, ref ResponseJson);
                _syslog.Log(LogLevel.Information,$"ReconcileFutureEnrollment: Woz Response =  {ResponseJson} ");

                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);

                string SectionId = string.Empty;
                string EnrollmentId = string.Empty;
                string SectionCourseCode = string.Empty;
                string EnrollStatus = string.Empty;

                for (int i = 0; i < ResponseObject.Count; ++i)
                {
                    SectionId = ResponseObject[i].sectionId.ToString();
                    EnrollmentId = ResponseObject[i].enrollmentId.ToString();
                    SectionCourseCode = GetCourseCodeFromSection(SectionId);
                    EnrollStatus = ResponseObject[i].enrollmentStatus.ToString();

                    if (SectionCourseCode == Enrollment.Course.Code)
                        break;

                }
                // Create the woz section for the enrollment and mark the enrollment as complete 
                if (SectionCourseCode == Enrollment.Course.Code)
                {

                    _syslog.Log(LogLevel.Information, $"ReconcileFutureEnrollment: Enrollment found!");
                    // Create woz course entrollment record 
                    WozCourseEnrollment wozCourseEnrollment = new WozCourseEnrollment()
                    {
                        IsDeleted = 0,
                        CreateDate = DateTime.Now,
                        ModifyDate = DateTime.Now,
                        ModifyGuid = Guid.NewGuid(),
                        WozCourseEnrollmentId = 0,
                        WozEnrollmentId = int.Parse(EnrollmentId),
                        SectionId = int.Parse(SectionId),
                        EnrollmentStatus = int.Parse(EnrollStatus),
                        ExeterId = int.Parse(StudentLogin.VendorLogin),
                        EnrollmentDateUTC = UTCNowUnixMilliseconds,
                        EnrollmentGuid = (Guid)Enrollment.EnrollmentGuid
                    };

                    _db.WozCourseEnrollment.Add(wozCourseEnrollment);
                    // Mark the entrollment as complete 
                    Enrollment.EnrollmentStatusId = (int)EnrollmentStatus.RegisterStudentComplete;
                    _db.SaveChanges();
                    _syslog.Log(LogLevel.Information,$"ReconcileFutureEnrollment: Enrollment reconciliation complete !");

                }
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                _syslog.Log(LogLevel.Error,$"ReconcileFutureEnrollment: Fatal Error! Exception = {ex.Message} ",true );
                return false;
            }
        }



        #endregion

   

        #region Utility Functions


        private string GetCourseCodeFromSection(string SectionId)
        {
            string Rval = string.Empty;
            try
            {
                string ResponseJson = string.Empty;
                ExecuteWozGet($"sections/{SectionId}", ref ResponseJson);
                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                Rval = ResponseObject.courseCode;
            }
            catch { }
            return Rval;
        }



        private HttpResponseMessage ExecuteWozPost(string ApiAction, string Content, ref string ResponseJson)
        {
            HttpClient client = CreateWozPostClient();
            HttpRequestMessage WozRequest = CreateWozPostRequest(ApiAction, Content);
            HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());
            return WozResponse;

        }

        private HttpResponseMessage ExecuteWozGet(string ApiAction, ref string ResponseJson)
        {

            HttpClient client = CreateWozGetClient();
            HttpRequestMessage WozRequest = CreateWozGetRequest(ApiAction);
            HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());
            return WozResponse;
        }


        private HttpRequestMessage CreateWozPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage CreateWozGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);
            return request;

        }
        
        private HttpClient CreateWozPostClient()
        {
            HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpPostClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private HttpClient CreateWozGetClient()
        {
            HttpClient client = _HttpClientFactory.CreateClient(Constants.HttpGetClientName);
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

        public static DateTime FromWozTime(long wozTime)
        {
            return epoch.AddMilliseconds(wozTime);
        }

        public static long ToWozTime(DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalMilliseconds;
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
