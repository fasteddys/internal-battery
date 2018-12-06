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
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyLib.MessageQueue;
using Microsoft.Extensions.Logging;

namespace UpDiddyApi.Workflow
{
    public class WorkflowHelper 
    {

        protected internal WozTransactionLog _log = null;
        protected internal UpDiddyDbContext _db = null;
        protected internal ILogger _sysLog = null;


        public WorkflowHelper(UpDiddyDbContext context, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger sysLog)
        {
            _db = context;
            _sysLog = sysLog;
        }

        public void WorkItemError(string EnrollmentGuid, MessageTransactionResponse Info)
        {
            // Log error to system logger 
            _sysLog.Log(LogLevel.Critical, $"Fatal error for enrollment {EnrollmentGuid}.  Info: {Info.ToString()}");
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
            _sysLog.Log(LogLevel.Critical, $"Error for enrollment {EnrollmentGuid}.  Info: {Info.ToString()} ");
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
            _sysLog.Log(LogLevel.Critical, $"Fatal error for enrollment {EnrollmentGuid}.  Info: {Info}");
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
            _sysLog.Log(LogLevel.Critical, $"Error for enrollment {EnrollmentGuid}.  Info: {Info}");
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

  

    }
}
