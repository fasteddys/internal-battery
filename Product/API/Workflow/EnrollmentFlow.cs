using AutoMapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UpDiddyApi.Business;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;

namespace UpDiddyApi.Workflow
{
    public class WozEnrollmentFlow
    {

        #region Class
        private UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private IConfiguration _configuration;

        private int _retrySeconds = 0;
        private int _wozVendorId = 0;
        public WozEnrollmentFlow(UpDiddyDbContext dbcontext, IMapper mapper, IConfiguration configuration)
        {
            // TODO putmagic numbers in appsettings
            _retrySeconds = 3;
            _wozVendorId = 1;
            _db = dbcontext;
            _mapper = mapper;
            _configuration = configuration;

        }
        #endregion

        #region Student Enrollment

        public async Task<MessageTransactionResponse> EnrollStudentWorkItem(string EnrollmentGuid)
        {            
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db,_configuration);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration);
                RVal =  woz.EnrollStudent(EnrollmentGuid); 
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        if (RVal.State == TransactionState.Error)
                        {
                            // Parse Woz error to determine if the user's email is already registered 
                            // If it is, move on to creating section
                            if (RVal.ResponseJson.IndexOf("The provided e-mail address") > 0 &&
                                RVal.ResponseJson.IndexOf("is currently in use") > 0   )
                            {
                                await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentComplete);
                                BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.GetSectionWorkItem(EnrollmentGuid));
                            }
                            else
                            {
                                await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentError);
                                Helper.WorkItemError(EnrollmentGuid, RVal.ResponseJson);
                            }                                
                        }
                        break;
                    case TransactionState.FatalError:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.ResponseJson);
                        break;
                    case TransactionState.InProgress:
                        string TransactionId = RVal.Data;
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentInProgress);
                        BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.EnrollStudentInProgressWorkItem(EnrollmentGuid,TransactionId));
                        break;
                    case TransactionState.Complete:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentComplete);
                        BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.GetSectionWorkItem(EnrollmentGuid));
                        break;
                }
            }
            catch ( Exception ex)
            {
                await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentError);
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, ex.Message);
            }
            return RVal;           
        }

              
        public async Task<MessageTransactionResponse> EnrollStudentInProgressWorkItem(string EnrollmentGuid, string TransactionId)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration);
                RVal = woz.TransactionStatus(EnrollmentGuid,TransactionId);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal.InformationalMessage + ":" +  RVal.ResponseJson);
                        break;
                    case TransactionState.FatalError:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.InProgress:
                        // TransactionStatus should NEVER return InProgress
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);                
                        break;
                    case TransactionState.Complete:                      
                        // Check Status of returned value
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            BackgroundJob.Schedule<WozEnrollmentFlow>(wi => wi.EnrollStudentInProgressWorkItem(EnrollmentGuid, TransactionId),TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {                               
                            string ExeterId = string.Empty;
                            string RegistrationUrl = string.Empty;
                            woz.ParseWozEnrollmentResource(RVal.ResponseJson, ref ExeterId, ref RegistrationUrl);
                            // Create and persist Vendor Login object
                            VendorStudentLoginDto StudentLogin = new VendorStudentLoginDto()
                            {
                                CreateDate = DateTime.Now,
                                ModifyDate = DateTime.Now,
                                IsDeleted = 0,
                                VendorId = _wozVendorId,
                                VendorLogin = ExeterId,
                                RegistrationUrl = RegistrationUrl
                            };
                            woz.CreateWozStudentLogin(StudentLogin, EnrollmentGuid);
                            await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentComplete);
                            // Move to next workitem
                            BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.GetSectionWorkItem(EnrollmentGuid));
                        }
                        else
                        {
                            await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                            Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        } 
                        break;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, ex.Message);
            }
            return RVal;
        }

        #endregion


        #region Create Section
   
        // Get a woz course section for the current enrollment  
        public async Task<MessageTransactionResponse> GetSectionWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration);
                RVal = woz.GetSectionForEnrollment(EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionError);
                        Helper.WorkItemError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.FatalError:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.InProgress:
                        string TransactionId = RVal.Data;
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionInProgress);
                        BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.CreateSectionInProgressWorkItem(EnrollmentGuid, TransactionId));
                        break;
                    case TransactionState.Complete:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionComplete);
                        BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterStudentWorkItem(EnrollmentGuid));
                        break;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, ex.Message);
            }
            return RVal;
        }
         
        // This function will wait until woz creates a new section for the enrollment, and save it to the CC database once created.
        public async Task<MessageTransactionResponse> CreateSectionInProgressWorkItem(string EnrollmentGuid,string TransactionId)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration);
                RVal = woz.TransactionStatus(EnrollmentGuid, TransactionId);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionError);
                        Helper.WorkItemError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.FatalError:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.InProgress:
                        // TransactionStatus should NEVER return InProgress
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.Complete:
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            BackgroundJob.Schedule<WozEnrollmentFlow>(wi => wi.CreateSectionInProgressWorkItem(EnrollmentGuid,TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {
                            // Create CourseSectionDto from information received from woz about newly created section 
                            WozCourseSectionDto CourseSectionDto  = woz.ParseWozSectionResource(RVal.ResponseJson);
                            // Save section to CC database 
                            woz.SaveCourseSection(CourseSectionDto, EnrollmentGuid);                           
                            // Move to next workitem
                            BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterStudentWorkItem(EnrollmentGuid));
                        }
                        else
                        {
                            await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionFatalError);
                            Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        }
                        break;                   
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, ex.Message);
            }
            return RVal;
        }



        #endregion


        #region Register Student


        // Register the student in a woz section to complete the enrollment process 
        public async Task<MessageTransactionResponse> RegisterStudentWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration);
                RVal = woz.RegisterStudent(EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.FatalError:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.InProgress:
                        string TransactionId = RVal.Data;
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentInProgress);
                        BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterStudentInProgressWorkItem(EnrollmentGuid, TransactionId));
                        break;
                    case TransactionState.Complete:
                        // Redundent registration, just mark it complete 
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentComplete);
                        break;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, ex.Message);
            }
            return RVal;
        }


        // Confirm that the student has been sucessfully enrolled 
        public async Task<MessageTransactionResponse> RegisterStudentInProgressWorkItem(string EnrollmentGuid, string TransactionId)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration);
                RVal = woz.TransactionStatus(EnrollmentGuid, TransactionId);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.FatalError:
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        break;
                    case TransactionState.InProgress:
                        BackgroundJob.Schedule<WozEnrollmentFlow>(wi => wi.RegisterStudentInProgressWorkItem(EnrollmentGuid, TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        break;
                    case TransactionState.Complete:
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            BackgroundJob.Schedule<WozEnrollmentFlow>(wi => wi.RegisterStudentInProgressWorkItem(EnrollmentGuid, TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {
                            // Create CourseSectionDto from information received from woz about newly created section 
                            WozCourseEnrollmentDto CourseEnrollmentDto = woz.ParseWozCourseEnrollmentResource(EnrollmentGuid, RVal.ResponseJson);
                            // Save section to CC database 
                             woz.SaveWozCourseEnrollment(EnrollmentGuid, CourseEnrollmentDto);
                            // Done, Whew!!
                            await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentComplete);


                        }
                        else
                        {
                            await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionFatalError);
                            Helper.WorkItemFatalError(EnrollmentGuid, RVal.InformationalMessage + ":" + RVal.ResponseJson);
                        }

                        break;

                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, ex.Message);
            }
            return RVal;
        }
 

        #endregion


    }



}
