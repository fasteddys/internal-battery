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
        public string WozU(string enrollmentGuid)
        {
            Console.WriteLine("***** - Enroll: " + enrollmentGuid);
            return "***** - Enroll: " + enrollmentGuid;
        }

        #region Student Enrollment

    public async Task<MessageTransactionResponse> EnrollStudentWorkItem(string EnrollmentGuid)
        {            
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper();
            try
            {
                Woz woz = new Woz(_db, _mapper, _configuration);

                woz.EnrollStudent(EnrollmentGuid);

                //TODO BRENT MAKE THIS INTO METHOD CALL IN new woz class!!!!! 
                RVal = await Helper.DoWorkItem("woz/EnrollStudent/" + EnrollmentGuid);

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
                                BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.CreateSectionWorkItem(EnrollmentGuid));
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
                        BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.CreateSectionWorkItem(EnrollmentGuid));
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

        /*
        Step 2:   Get the status of the the students enrollment in Woz

EndPoint: http://localhost:5001/api/woz/TransactionStatus/d0de919b-ee4e-4514-9ee7-434786841203/fa7ac26d-0c3d-43b4-ab9d-1b48b2d081b7

Return Object:
{
"informationalMessage":"Transaction Complete",
"state":0,
"data":"400",
"responseJson":"{"transactionId":"fa7ac26d-0c3d-43b4-ab9d-1b48b2d081b7","clientId":"allegis.qa.exeterlms.com","createDateUTC":1537873016476,"lastOperationDateUTC":1537873024607,"expectedCompletionDateUTC":253402300800000,"operation":"UserCreate","status":400,"message":"The operation has completed with the following message: User Email: ikoplowitz @populusgroup.com Id: 5b237d37-ef06-495f-88a9-5ba3ecbb5ec3 registration successful!","resource":"{ "invitationCode":"a056bc5f-7d76-400d-b6a1-7eaa1e84a69f","registrationUrl":"https://allegis.qa.exeterlms.com/Account/Register?invitationCode=a056bc5f-7d76-400d-b6a1-7eaa1e84a69f","exeterId":3237,"firstName":"Ian","lastName":"Koplowitz","emailAddress":"ikoplowitz@populusgroup.com","integrationIds":null}","apiVersion":"1.0"}"
}

    Note: Controller must persist student vendor login data with a call to /SaveStudentEnrollment/{EnrollmentGuid
}
passing a VendorStudentLogin object in the body"
Note: Controller must HTTP PUT a EnrollmentDTO object to /api/enrollment/" with an updated enrollment status 

    */

        //TODO Finish
        public async Task<MessageTransactionResponse> EnrollStudentInProgressWorkItem(string EnrollmentGuid, string TransactionId)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper();
            try
            {
                RVal = await Helper.DoWorkItem("woz/TransactionStatus/" + EnrollmentGuid + "/" + TransactionId);
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
                        await Helper.UpdateEnrollmentStatus(EnrollmentGuid, UpDiddyLib.Dto.EnrollmentStatus.EnrollStudentComplete);
                        // Check Status of returned value
                        int WozTransactionStatus = int.Parse(RVal.Data);
                        // < 400 try again (See Woz documentation for their status codes)
                        if (WozTransactionStatus < 400)
                            BackgroundJob.Schedule<WozEnrollmentFlow>(wi => wi.CreateSectionWorkItem(EnrollmentGuid),TimeSpan.FromSeconds(_retrySeconds));
                        else if (WozTransactionStatus == 400)
                        {
                            // Create and persist Vendor Login object
                            VendorStudentLoginDto StudentLogin = new VendorStudentLoginDto();
                            string ExeterId = string.Empty;
                            string RegistrationUrl = string.Empty;
                            Helper.ParseWozEnrollmentResource(RVal.ResponseJson, ref ExeterId, ref RegistrationUrl);
                            StudentLogin.CreateDate = DateTime.Now;
                            StudentLogin.ModifyDate = DateTime.Now;
                            StudentLogin.IsDeleted = 0;
                            StudentLogin.VendorId = _wozVendorId;
                            StudentLogin.VendorLogin = ExeterId;
                            StudentLogin.RegistrationUrl = RegistrationUrl;              
                            await Helper.CreateWozStudentLogin(StudentLogin, EnrollmentGuid);                            
                            // Move to next workitem
                            BackgroundJob.Enqueue<WozEnrollmentFlow>(wi => wi.CreateSectionWorkItem(EnrollmentGuid));
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

        /*
                Step 3:   Create the section for the course in woz

                EndPoint: http://localhost:5001/api/woz/createsection/d0de919b-ee4e-4514-9ee7-434786841203

        Return Object:
        {
        "informationalMessage":"The request to modify LMS data was successfully queued for processing.  The status of the request may be monitored with the following transaction identifier: '8a00be19-0815-419d-9719-8ac4e1d296fa'.",
        "state":1,
        "data":"8a00be19-0815-419d-9719-8ac4e1d296fa",
        "responseJson":"{"transactionId":"8a00be19-0815-419d-9719-8ac4e1d296fa","payload":null,"payloadType":null,"message":"The request to modify LMS data was successfully queued for processing.The status of the request may be monitored with the following transaction identifier: '8a00be19-0815-419d-9719-8ac4e1d296fa'."}"
        }


        Note: Controller must HTTP PUT a EnrollmentDTO object to /api/enrollment/UpdateEnrollmentStatus{EnrollmentGuid/{Enrollment Status}" with an updated enrollment status 
            */

        //TODO Finish
        public async Task<MessageTransactionResponse> CreateSectionWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper();
            try
            {
               // RVal = await Helper.DoWorkItem("woz/EnrollStudent/" + EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:

                        break;
                    case TransactionState.FatalError:

                        break;
                    case TransactionState.InProgress:

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



        /*

                Step 4:   Get the status of the the section creation

        EndPoint: http://localhost:5001/api/woz/TransactionStatus/d0de919b-ee4e-4514-9ee7-434786841203/8a00be19-0815-419d-9719-8ac4e1d296fa

        Return Object:
        {
        "informationalMessage":"Transaction Complete",
        "state":0,
        "data":"400",
        "responseJson":"{"transactionId":"8a00be19-0815-419d-9719-8ac4e1d296fa","clientId":"allegis.qa.exeterlms.com","createDateUTC":1537875919749,"lastOperationDateUTC":1537875923027,"expectedCompletionDateUTC":253402300800000,"operation":"SectionCreate","status":400,"message":"The operation has completed with the following message: Section Created","resource":"{"sectionId":42519,"createDateUTC":1537875921065,"startDateUTC":1468800000,"endDateUTC":1468800000,"isOpen":true,"maxStudents":0,"timeZone":"Eastern Standard Time","courseCode":"FSWG100","courseName":"Coding From Scratch"}","apiVersion":"1.0"}"
        }



        Note: Controller must HTTP PUT a EnrollmentDTO object to /api/enrollment/UpdateEnrollmentStatus{EnrollmentGuid/{Enrollment Status}" with an updated enrollment status 
        }
        passing a WozCourseSection object in the body"
        Note: Controller must HTTP PUT a EnrollmentDTO object to /api/enrollment/" with an updated enrollment status 
        */

        //TODO Finish
        public async Task<MessageTransactionResponse> CreateSectionInProgressWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper();
            try
            {
                RVal = await Helper.DoWorkItem("woz/EnrollStudent/" + EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:

                        break;
                    case TransactionState.FatalError:

                        break;
                    case TransactionState.InProgress:

                        break;
                    case TransactionState.Complete:
                        // Need to create recored in wozCourseSection  for the given course  
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


        /*

                Step 5:   Register the student in a section

        EndPoint: http://localhost:5001/api/woz/RegisterStudent/d0de919b-ee4e-4514-9ee7-434786841203

        Return Object:
        {
        "informationalMessage":"The request to modify LMS data was successfully queued for processing.  The status of the request may be monitored with the following transaction identifier: '5504612d-672e-4836-a536-d2affffa1d42'.",
        "state":1,
        "data":"5504612d-672e-4836-a536-d2affffa1d42",
        "responseJson":"{"transactionId":"5504612d-672e-4836-a536-d2affffa1d42","payload":null,"payloadType":null,"message":"The request to modify LMS data was successfully queued for processing.The status of the request may be monitored with the following transaction identifier: '5504612d-672e-4836-a536-d2affffa1d42'."}"
        }

      
Note: Controller must HTTP PUT a EnrollmentDTO object to /api/enrollment/UpdateEnrollmentStatus{EnrollmentGuid/{Enrollment Status}" with an updated enrollment status 

        */

        //TODO Finish
        public async Task<MessageTransactionResponse> RegisterStudentWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper();
            try
            {
                RVal = await Helper.DoWorkItem("woz/EnrollStudent/" + EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:

                        break;
                    case TransactionState.FatalError:

                        break;
                    case TransactionState.InProgress:

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


        /*
                Step 6:   Get the status of the registration

        EndPoint: http://localhost:5001/api/woz/TransactionStatus/d0de919b-ee4e-4514-9ee7-434786841203/5504612d-672e-4836-a536-d2affffa1d42

        Return Object:
        {
        "informationalMessage":"Transaction Complete",
        "state":0,
        "data":"400",
        "responseJson":"{"transactionId":"5504612d-672e-4836-a536-d2affffa1d42","clientId":"allegis.qa.exeterlms.com","createDateUTC":1537878414074,"lastOperationDateUTC":1537878415242,"expectedCompletionDateUTC":253402300800000,"operation":"CourseEnrollmentCreate","status":400,"message":"The operation has completed with the following message: Enrollment Created","resource":"{\\"enrollmentId\\":17509,\\"sectionId\\":42519,\\"enrollmentStatus\\":100,\\"enrollmentDateUTC\\":1468800000,\\"removalDateUTC\\":null,\\"exeterId\\":3237,\\"firstName\\":\\"Ian\\",\\"lastName\\":\\"Koplowitz\\",\\"emailAddress\\":\\"ikoplowitz@populusgroup.com\\",\\"integrationIds\\":null}","apiVersion":"1.0"}"
        }


        Note: Controller must HTTP PUT a EnrollmentDTO object to /api/enrollment/UpdateEnrollmentStatus{EnrollmentGuid/{Enrollment Status}" with an updated enrollment status 
        Note: Controller must call /SaveWozEnrollment/{EnrollmentGuid
        }
        with a WozEnrollmentDto object to save woz specific enrollment data returned by Woz in the Resource object

            */



        //TODO Finish
        public async Task<MessageTransactionResponse> RegisterStudentInProgressWorkItem(string EnrollmentGuid)
        {
            MessageTransactionResponse RVal = null;
            WorkflowHelper Helper = new WorkflowHelper();
            try
            {
                RVal = await Helper.DoWorkItem("woz/EnrollStudent/" + EnrollmentGuid);
                switch (RVal.State)
                {
                    case TransactionState.Error:

                        break;
                    case TransactionState.FatalError:

                        break;
                    case TransactionState.InProgress:

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
