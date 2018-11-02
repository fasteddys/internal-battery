using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.Business;
using UpDiddyApi.Models;
//using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyLib.MessageQueue;

namespace UpDiddyApi.Workflow
{
    public class WorkflowHelper 
    {

        protected internal WozTransactionLog _log = null;
        protected internal UpDiddyDbContext _db = null;
        protected internal string _apiBaseUri = null;
        protected internal ISysLog _sysLog = null;


        public WorkflowHelper(UpDiddyDbContext context, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysLog sysLog)
        {
            // TODO Get URI from appsetting.json 
            _apiBaseUri = "http://localhost:5001/api/";   // Is this still neeed      
            _db = context;
            _sysLog = sysLog;
        }

        public void WorkItemError(string EnrollmentGuid, MessageTransactionResponse Info)
        {
            // Log error to system logger 
            _sysLog.SysError($"Fatal error for enrollment {EnrollmentGuid}.  Info: {Info.ToString()} ", true);
            // Log Error to woz transaction log 
            _log = new WozTransactionLog();
            _log.EndPoint = "Error";
            _log.InputParameters = $"enrollmentGuid={EnrollmentGuid}";
            if (_log.WozTransactionLogId > 0)
                _log.WozTransactionLogId = 0;
            _log.ModifyDate = DateTime.Now;
            _log.CreateDate = DateTime.Now;
            _log.CreateGuid = Guid.NewGuid();
            _log.ModifyGuid = Guid.NewGuid();
            _log.ResponseJson = Info.ToString();
            _db.WozTransactionLog.Add(_log);
            _db.SaveChanges();
        }
        public void WorkItemFatalError(string EnrollmentGuid, MessageTransactionResponse Info)
        {
            // Log error to system logger 
            _sysLog.SysError($"Error for enrollment {EnrollmentGuid}.  Info: {Info.ToString()} ", true);
            // Log Error to woz transaction log 
            _log = new WozTransactionLog();
            _log.EndPoint = "FatalError";
            _log.InputParameters = $"enrollmentGuid={EnrollmentGuid}";
            if (_log.WozTransactionLogId > 0)
                _log.WozTransactionLogId = 0;
            _log.ModifyDate = DateTime.Now;
            _log.CreateDate = DateTime.Now;
            _log.CreateGuid = Guid.NewGuid();
            _log.ModifyGuid = Guid.NewGuid();
            _log.ResponseJson = Info.ToString();
            _db.WozTransactionLog.Add(_log);
            _db.SaveChanges();
        }

        public void WorkItemError(string EnrollmentGuid, string Info)
        {
            // Log error to system logger 
            _sysLog.SysError($"Fatal error for enrollment {EnrollmentGuid}.  Info: {Info} ", true);
            // Log Error to woz transaction log 
            _log = new WozTransactionLog();
            _log.EndPoint = "Error";
            _log.InputParameters = $"enrollmentGuid={EnrollmentGuid}";
            if (_log.WozTransactionLogId > 0)
                _log.WozTransactionLogId = 0;
            _log.ModifyDate = DateTime.Now;
            _log.CreateDate = DateTime.Now;
            _log.CreateGuid = Guid.NewGuid();
            _log.ModifyGuid = Guid.NewGuid();
            _log.ResponseJson = Info;
            _db.WozTransactionLog.Add(_log);
            _db.SaveChanges();
        }
        public void WorkItemFatalError(string EnrollmentGuid, string Info)
        {
            // Log error to system logger 
            _sysLog.SysError($"Error for enrollment {EnrollmentGuid}.  Info: {Info} ", true);
            // Log Error to woz transaction log 
            _log = new WozTransactionLog();
            _log.EndPoint = "FatalError";
            _log.InputParameters = $"enrollmentGuid={EnrollmentGuid}";
            if (_log.WozTransactionLogId > 0)
                _log.WozTransactionLogId = 0;
            _log.ModifyDate = DateTime.Now;
            _log.CreateDate = DateTime.Now;
            _log.CreateGuid = Guid.NewGuid();
            _log.ModifyGuid = Guid.NewGuid();
            _log.ResponseJson = Info;
            _db.WozTransactionLog.Add(_log);
            _db.SaveChanges();
        }



        public async Task<MessageTransactionResponse> DoWorkItem(string ApiAction)
        {
            HttpClient Client = ApiClient();
            HttpRequestMessage Request = ApiGetRequest(ApiAction);
            HttpResponseMessage Response = await Client.SendAsync(Request);
            var ResponseJson = await Response.Content.ReadAsStringAsync();
            MessageTransactionResponse RVal = JsonConvert.DeserializeObject<MessageTransactionResponse>(await Response.Content.ReadAsStringAsync());
            return RVal;
        }


 
        public string UpdateEnrollmentStatus(string EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus status)
        {
            Enrollment Enrollment = _db.Enrollment
                     .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                      .FirstOrDefault();

            if (Enrollment != null)
            {
                // Update the enrollment status and update the modify date 
                Enrollment.EnrollmentStatusId = (int)status;
                Enrollment.ModifyDate = DateTime.Now;
                _db.SaveChanges();
                return $"Enrollment {EnrollmentGuid} updated to {status}";
            }
            else
                return $"Enrollment {EnrollmentGuid} is not found";
        }

 


        private HttpRequestMessage ApiPutRequest(string ApiAction, string Content = "")
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, _apiBaseUri + ApiAction);

            if ( Content.Length > 0 )
            {
                request.Content = new StringContent(Content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
                
            return request;
        }

        private  HttpRequestMessage ApiPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage ApiGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);
            return request;

        }


        private HttpClient ApiClient()
        {
            HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }


    }
}
