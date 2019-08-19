using AutoMapper;
using Hangfire;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyLib.MessageQueue;
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Workflow
{
    public class WozEnrollmentFlow
    {

        #region Class
        private UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private IConfiguration _configuration;
        private ISysEmail _sysEmail = null;
        private ILogger _sysLog = null;
        private int _retrySeconds = 0;
        private int _wozVendorId = 0;
        private IHttpClientFactory _httpClientFactory = null;
        private IRepositoryWrapper _repositoryWrapper;
        private ITaggingService _taggingService;
        private readonly IHangfireService _hangfireService;


        public WozEnrollmentFlow(UpDiddyDbContext dbcontext, 
            IMapper mapper, 
            IConfiguration configuration,
            ISysEmail sysEmail, 
            IServiceProvider serviceProvider, 
            IHttpClientFactory httpClientFactory, 
            ILogger<WozEnrollmentFlow> logger, 
            IRepositoryWrapper repositoryWrapper,
            ITaggingService taggingService,
            IHangfireService hangfireService)
        {
            _retrySeconds = int.Parse(configuration["Woz:RetrySeconds"]);
            // TODO modify code to work off woz Guid not dumb key 
            _wozVendorId = int.Parse(configuration["Woz:VendorId"]);
            _db = dbcontext;
            _mapper = mapper;
            _configuration = configuration;
            _sysEmail = sysEmail;
            _sysLog = logger;
            _httpClientFactory = httpClientFactory;
            _repositoryWrapper = repositoryWrapper;
            _taggingService = taggingService;
            _hangfireService = hangfireService;
        }
        #endregion

        #region Student Enrollment

        public async Task<MessageTransactionResponse> EnrollStudentWorkItem(string EnrollmentGuid, int SubscriberId)
        {

            MessageTransactionResponse RVal = null;
          
            WorkflowHelper Helper = new WorkflowHelper(_db,_configuration,_sysLog);
            bool IsInstructorLed = false;
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
               
                RVal =  woz.EnrollStudent(EnrollmentGuid, ref IsInstructorLed);
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
                                Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentComplete);
                                _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.GetSectionWorkItem(EnrollmentGuid));
                            }
                            else
                            {
                                Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentError);
                                Helper.WorkItemError(EnrollmentGuid, RVal);
                            }                                
                        }
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        string TransactionId = RVal.Data;
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentInProgress);
                        _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.EnrollStudentInProgressWorkItem(EnrollmentGuid,TransactionId,IsInstructorLed, SubscriberId));
                        break;
                    case TransactionState.Complete:
                        if ( IsInstructorLed == false )
                            Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentComplete);
                        else
                            Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentRequested);
                        _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.EnrollStudentCompleteWorkItem(EnrollmentGuid,IsInstructorLed, SubscriberId));
                        break;
                }
                RVal.Step = 4;
            }
            catch ( Exception ex)
            {
                Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentError);
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;           
        }

              
        public async Task<MessageTransactionResponse> EnrollStudentInProgressWorkItem(string EnrollmentGuid, string TransactionId, bool IsInstructorLed, int SubscriberId)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
                RVal = woz.TransactionStatus(EnrollmentGuid,TransactionId);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        // TransactionStatus should NEVER return InProgress
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);                
                        break;
                    case TransactionState.Complete:                      
                        // Check Status of returned value
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            _hangfireService.Schedule<WozEnrollmentFlow>(wi => wi.EnrollStudentInProgressWorkItem(EnrollmentGuid, TransactionId,IsInstructorLed, SubscriberId),TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {                               
                            string ExeterId = string.Empty;
                            string RegistrationUrl = string.Empty;
                            woz.ParseWozEnrollmentResource(RVal.ResponseJson, ref ExeterId, ref RegistrationUrl);
                            // Create and persist Vendor Login object
                            VendorStudentLoginDto StudentLogin = new VendorStudentLoginDto()
                            {
                                CreateGuid = Guid.Empty,
                                ModifyGuid = Guid.Empty,
                                CreateDate = DateTime.UtcNow,
                                ModifyDate = DateTime.UtcNow,
                                IsDeleted = 0,
                                VendorId = _wozVendorId,
                                VendorLogin = ExeterId,
                                RegistrationUrl = RegistrationUrl
                            };
                            woz.CreateWozStudentLogin(StudentLogin, EnrollmentGuid);
                            if (IsInstructorLed == false)
                                Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentComplete);
                            else
                                Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentRequested);

                            // Move to next workitem
                            _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.EnrollStudentCompleteWorkItem(EnrollmentGuid,IsInstructorLed, SubscriberId));
                        }
                        else
                        {
                            Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                            Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        } 
                        break;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;
        }


        public async Task<MessageTransactionResponse> EnrollStudentCompleteWorkItem(string EnrollmentGuid, bool IsInstructorLed, int SubscriberId)
        {

            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            // Get the Enrollment Object 
            Enrollment Enrollment = _db.Enrollment
                 .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                 .FirstOrDefault();

            if ( Enrollment == null )
            {
                Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                Helper.WorkItemFatalError(EnrollmentGuid, "EnrollStudentCompleteWorkItem: Cannot located enrollment" );
            }
            else
            {
                Group WozStudentGroup = _repositoryWrapper.GroupRepository.GetGroupByName(UpDiddyLib.Helpers.Constants.CrossReference.Group.WOZ_STUDENT);
                await _taggingService.AddSubscriberToGroupAsync(WozStudentGroup.GroupId, SubscriberId);

                // Use a different flow for instructor led courses versus self-paced 
                if (Enrollment.EnrollmentStatusId == (int)EnrollmentStatus.FutureRegisterStudentRequested)
                    _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterInstructorLedStudentWorkItem(EnrollmentGuid));
                else if (Enrollment.EnrollmentStatusId == (int)EnrollmentStatus.EnrollStudentComplete)
                    _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.GetSectionWorkItem(EnrollmentGuid));
                else if (Enrollment.EnrollmentStatusId == (int)EnrollmentStatus.FutureRegisterStudentComplete)
                    ; // Do nothing 
                else
                {
                    Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                    Helper.WorkItemFatalError(EnrollmentGuid, $"EnrollStudentCompleteWorkItem: {Enrollment.EnrollmentStatusId} is not an valid Enrollment Status");
                }
            
            }

            return new MessageTransactionResponse()
            {
                InformationalMessage = string.Empty,
                Data = string.Empty,
                ResponseJson = string.Empty,
                State = TransactionState.Complete                
            };
        }

            #endregion


        #region Create Section

            // Get a woz course section for the current enrollment  
            public async Task<MessageTransactionResponse> GetSectionWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
                RVal = woz.GetSectionForEnrollment(EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionError);
                        Helper.WorkItemError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        string TransactionId = RVal.Data;
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionInProgress);
                        _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.CreateSectionInProgressWorkItem(EnrollmentGuid, TransactionId));
                        break;
                    case TransactionState.Complete:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionComplete);
                        _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterStudentWorkItem(EnrollmentGuid));
                        break;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;
        }
         
        // This function will wait until woz creates a new section for the enrollment, and save it to the CC database once created.
        public async Task<MessageTransactionResponse> CreateSectionInProgressWorkItem(string EnrollmentGuid,string TransactionId)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
                RVal = woz.TransactionStatus(EnrollmentGuid, TransactionId);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionError);
                        Helper.WorkItemError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        // TransactionStatus should NEVER return InProgress
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.Complete:
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            _hangfireService.Schedule<WozEnrollmentFlow>(wi => wi.CreateSectionInProgressWorkItem(EnrollmentGuid,TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {
                            // Create CourseSectionDto from information received from woz about newly created section 
                            WozCourseSectionDto CourseSectionDto  = woz.ParseWozSectionResource(RVal.ResponseJson);
                            // Save section to CC database 
                            woz.SaveCourseSection(CourseSectionDto, EnrollmentGuid);                           
                            // Move to next workitem
                            _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterStudentWorkItem(EnrollmentGuid));
                        }
                        else
                        {
                            Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.CreateSectionFatalError);
                            Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        }
                        break;                   
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;
        }



        #endregion

        #region Register instructor led course

        public async Task<MessageTransactionResponse> RegisterInstructorLedStudentWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
                RVal = woz.RegisterStudentInstructorLed(EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        string TransactionId = RVal.Data;
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentInProgress);
                        _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterInstructorLedStudentInProgressWorkItem(EnrollmentGuid, TransactionId));
                        break;
                    case TransactionState.Complete:
                        // Redundent registration, just mark it complete 
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentComplete);
                        break;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;


        }

        // Confirm that the student has been sucessfully enrolled for an instructor led course 
        public async Task<MessageTransactionResponse> RegisterInstructorLedStudentInProgressWorkItem(string EnrollmentGuid, string TransactionId)
        {


            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
                RVal = woz.TransactionStatus(EnrollmentGuid, TransactionId);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        _hangfireService.Schedule<WozEnrollmentFlow>(wi => wi.RegisterInstructorLedStudentInProgressWorkItem(EnrollmentGuid, TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        break;
                    case TransactionState.Complete:
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            _hangfireService.Schedule<WozEnrollmentFlow>(wi => wi.RegisterInstructorLedStudentInProgressWorkItem(EnrollmentGuid, TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {
                            // Mark the enrollment complete and the reconcilation service will finish wiring everything up once woz create the 
                            // course and section
                            Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentComplete);
                        }
                        else
                        {
                            Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentFatalError);
                            Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        }

                        break;

                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;
        }

        #endregion


        #region Register Student

        // Register the student in a woz section to complete the enrollment process 
        public async Task<MessageTransactionResponse> RegisterStudentWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
                RVal = woz.RegisterStudent(EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        string TransactionId = RVal.Data;
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentInProgress);
                        _hangfireService.Enqueue<WozEnrollmentFlow>(wi => wi.RegisterStudentInProgressWorkItem(EnrollmentGuid, TransactionId));
                        break;
                    case TransactionState.Complete:
                        // Redundent registration, just mark it complete 
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentComplete);
                        break;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;
        }


        // Confirm that the student has been sucessfully enrolled 
        public async Task<MessageTransactionResponse> RegisterStudentInProgressWorkItem(string EnrollmentGuid, string TransactionId)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            try
            {
                WozInterface woz = new WozInterface(_db, _mapper, _configuration, _sysLog, _httpClientFactory);
                RVal = woz.TransactionStatus(EnrollmentGuid, TransactionId);
                switch (RVal.State)
                {
                    case TransactionState.Error:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentError);
                        Helper.WorkItemError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.FatalError:
                        Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentFatalError);
                        Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        break;
                    case TransactionState.InProgress:
                        _hangfireService.Schedule<WozEnrollmentFlow>(wi => wi.RegisterStudentInProgressWorkItem(EnrollmentGuid, TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        break;
                    case TransactionState.Complete:
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            _hangfireService.Schedule<WozEnrollmentFlow>(wi => wi.RegisterStudentInProgressWorkItem(EnrollmentGuid, TransactionId), TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {
                            // Create CourseSectionDto from information received from woz about newly created section 
                            WozCourseEnrollmentDto CourseEnrollmentDto = woz.ParseWozCourseEnrollmentResource(EnrollmentGuid, RVal.ResponseJson);
                            // Save section to CC database 
                             woz.SaveWozCourseEnrollment(EnrollmentGuid, CourseEnrollmentDto);
                            // Done, Whew!!
                             Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentComplete);
                            // Todo consume any promo codes here
                            // Todo Send "congrats you've enrolled" email

                        }
                        else
                        {
                            Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentFatalError);
                            Helper.WorkItemFatalError(EnrollmentGuid, RVal);
                        }

                        break;

                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                Helper.WorkItemError(EnrollmentGuid, RVal);
            }
            return RVal;
        }


        #endregion

        


    }



}
