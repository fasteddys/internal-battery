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

namespace UpDiddyApi.Business
{
    public class Woz : BusinessVendorBase
    {
        private readonly string _ApiBaseUri;

        public Woz(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["Woz:ApiUrl"];
            _accessToken = configuration["WozAccessToken"];
        }

        public MessageTransactionResponse EnrollStudent(string enrollmentGuid)
        {
            

            _log = new WozTransactionLog();

            _log.EndPoint = "Woz::EnrollStudent";
            _log.InputParameters = $"Woz::EnrollStudent::enrollmentGuid={enrollmentGuid}";

            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == enrollmentGuid)
                 .FirstOrDefault();

            // Check the validity of the request 
            if (Enrollment == null)
                return CreateResponse(string.Empty, $"Enrollment {enrollmentGuid} was not found.", enrollmentGuid, TransactionState.FatalError);

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

            HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));

            SendEmail("brferree@allegisgroup.com", "test via sendgrid", "<html><body>content</body></html>");



            string ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());

            _log.WozResponseJson = ResponseJson;

            return new MessageTransactionResponse();
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

            if (_log.WozTransactionLogId > 0) _log.WozTransactionLogId = 0;
            _log.ResponseJson = RValJson;
            _log.ModifyDate = DateTime.Now;
            _log.CreateDate = DateTime.Now;
            _log.CreateGuid = Guid.NewGuid();
            _log.ModifyGuid = Guid.NewGuid();

            _db.WozTransactionLog.Add(_log);
            _db.SaveChanges();

            return RVal;
        }


        private bool SendEmail(string email, string subject, string htmlContent)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@careercircle.com", "CareerCircle Support");
            var to = new EmailAddress(email);
            var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = AsyncHelper.RunSync<Response>(() => client.SendEmailAsync(msg));
            
            return true;
        }


    }



    #region async helper

    internal static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }

    #endregion


}
