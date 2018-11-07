using AutoMapper;
using Braintree;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.Business;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyLib.Helpers.Braintree;
using UpDiddyLib.MessageQueue;
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;

namespace UpDiddyApi.Workflow
{
    public class BraintreePaymentFlow
    {

        private UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private IConfiguration _configuration;
        private ISysEmail _sysEmail = null;
        private ISysLog _sysLog = null;
        private IBraintreeConfiguration _braintreeConfiguration = null;

        public BraintreePaymentFlow(UpDiddyDbContext dbcontext, IMapper mapper, IConfiguration configuration, ISysEmail sysEmail, ISysLog sysLog)
        {
            _db = dbcontext;
            _mapper = mapper;
            _configuration = configuration;
            _sysEmail = sysEmail;
            _sysLog = sysLog;
            _braintreeConfiguration = new BraintreeConfiguration(_configuration);
        }

        public async Task<MessageTransactionResponse> PaymentWorkItem(EnrollmentFlowDto EnrollmentFlowDto)
        {

            // Extract the two DTOs from the DTO that's passed in.
            BraintreePaymentDto BraintreePaymentDto = null;
            EnrollmentDto EnrollmentDto = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            string ErrorsEmailAddress = _configuration["EmailAddresses:Errors"];

            if (string.IsNullOrEmpty(ErrorsEmailAddress))
            {
                _sysLog.SysError("No error email is supplied in the application settings; error emails will be sent to errors@careercircle.com by default.");
                ErrorsEmailAddress = "errors@careercircle.com";
            }

            try
            {
                BraintreePaymentDto = EnrollmentFlowDto.BraintreePaymentDto;
                EnrollmentDto = EnrollmentFlowDto.EnrollmentDto;
            }
            catch (Exception e)
            {
                string ErrorMessage = "BraintreePaymentFlow:PaymentWorkItem -> No Braintree or enrollment dto present; enrollment has been cancelled.";
                _sysLog.SysError(ErrorMessage);
                _sysEmail.SendEmail(ErrorsEmailAddress, "An error has occurred in the enrollment flow", CreateErrorEmail(ErrorMessage, EnrollmentFlowDto));
                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentError);
                return CreateResponse(CreateResponseJson(ErrorMessage), ErrorMessage, string.Empty, TransactionState.FatalError);
            }


            IBraintreeGateway gateway = _braintreeConfiguration.GetGateway();
            string nonce = BraintreePaymentDto.Nonce;
            TransactionRequest TransactionRequest = null;

            // Make sure we don't get null object exceptions when referencing DTOs
            

            // Make sure Nonce is present
            if (string.IsNullOrEmpty(nonce))
            {
                string NullOrEmptyMessage = "BraintreePaymentFlow:PaymentWorkItem -> Braintree nonce is null or empty; enrollment has been cancelled.";
                _sysLog.SysError(NullOrEmptyMessage);
                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentError);
                _sysEmail.SendEmail(ErrorsEmailAddress, "An error has occurred in the enrollment flow", CreateErrorEmail(NullOrEmptyMessage, EnrollmentFlowDto));
                return CreateResponse(CreateResponseJson(NullOrEmptyMessage), NullOrEmptyMessage, string.Empty, TransactionState.FatalError);
            }

            // Create the transaction request object
            try
            {
                TransactionRequest = AssembleTransactionRequest(BraintreePaymentDto);
            }
            catch (Exception e)
            {
                string ExceptionString = "BraintreePaymentFlow:PaymentWorkItem -> An error occurred when attempting to create the TransactionRequest.";
                _sysLog.SysError(ExceptionString + e.Message);
                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentError);
                _sysEmail.SendEmail(ErrorsEmailAddress, "An error has occurred in the enrollment flow", CreateErrorEmail(ExceptionString, EnrollmentFlowDto));
                return CreateResponse(CreateResponseJson(ExceptionString), ExceptionString, string.Empty, TransactionState.FatalError);
            }

            try
            {
                Result<Transaction> result = gateway.Transaction.Sale(TransactionRequest);
                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentComplete);
                _sysLog.SysInfo("BraintreePaymentFlow:PaymentWorkItem -> Braintree payment in progress.");

                if (result.IsSuccess())
                {
                    string SuccessfulMessage = "BraintreePaymentFlow:PaymentWorkItem->Braintree payment was successfull.";
                    _sysLog.SysInfo(SuccessfulMessage);
                    SetSelfPacedOrInstructorLedStatus(Helper, EnrollmentDto);
                    BackgroundJob.Enqueue<WozEnrollmentFlow>(x => x.EnrollStudentWorkItem(EnrollmentDto.EnrollmentGuid.ToString()));
                    return CreateResponse(CreateResponseJson(SuccessfulMessage), SuccessfulMessage, string.Empty, TransactionState.Complete);
                }
                else
                {
                    string UnsuccessfulMessage = "BraintreePaymentFlow:PaymentWorkItem->Braintree payment was unsuccessful.";
                    _sysLog.SysError(UnsuccessfulMessage);
                    Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentFatalError);
                    _sysEmail.SendEmail(ErrorsEmailAddress, "An error has occurred in the enrollment flow", CreateErrorEmail(UnsuccessfulMessage, EnrollmentFlowDto));
                    return CreateResponse(CreateResponseJson(UnsuccessfulMessage), UnsuccessfulMessage, string.Empty, TransactionState.Error);
                }

            }
            catch (Exception ex)
            {
                string ExceptionString = "BraintreePaymentFlow:PaymentWorkItem -> An error occurred";
                _sysLog.SysError(ExceptionString + ex.Message);
                _sysEmail.SendEmail(ErrorsEmailAddress, "An error has occurred in the enrollment flow", CreateErrorEmail(ExceptionString, EnrollmentFlowDto));
                return CreateResponse(CreateResponseJson(ExceptionString), ExceptionString, string.Empty, TransactionState.FatalError);
            }
        }

        private void SetSelfPacedOrInstructorLedStatus(WorkflowHelper Helper, EnrollmentDto Enrollment)
        {
            Helper.UpdateEnrollmentStatus(Enrollment.EnrollmentGuid.ToString(), (EnrollmentStatus)Enrollment.EnrollmentStatusId);
        }

        private TransactionRequest AssembleTransactionRequest(BraintreePaymentDto BraintreePaymentObject)
        {
            TransactionRequest request = new TransactionRequest
            {
                Amount = BraintreePaymentObject.PaymentAmount,
                MerchantAccountId = _braintreeConfiguration.GetConfigurationSetting("BraintreeMerchantAccountID"),
                PaymentMethodNonce = BraintreePaymentObject.Nonce,
                Customer = new CustomerRequest
                {
                    FirstName = BraintreePaymentObject.FirstName,
                    LastName = BraintreePaymentObject.LastName,
                    Phone = BraintreePaymentObject.PhoneNumber,
                    Email = BraintreePaymentObject.Email
                },
                BillingAddress = new AddressRequest
                {
                    FirstName = BraintreePaymentObject.FirstName,
                    LastName = BraintreePaymentObject.LastName,
                    StreetAddress = BraintreePaymentObject.Address,
                    Region = BraintreePaymentObject.Region,
                    Locality = BraintreePaymentObject.Locality,
                    PostalCode = BraintreePaymentObject.ZipCode,
                    CountryCodeAlpha2 = BraintreePaymentObject.CountryCode
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
            };
            return request;
        }

        private string CreateErrorEmail(string ErrorMessage, EnrollmentFlowDto EnrollmentFlow)
        {
            StringBuilder sb = new StringBuilder(ErrorMessage);
            sb.Append(" | For Subscriber: " + EnrollmentFlow.SubscriberDto.SubscriberGuid);
            sb.Append(" | Enrollment: " + EnrollmentFlow.EnrollmentDto.EnrollmentGuid);
            return sb.ToString();
        }

        private string CreateResponseJson(string Response)
        {
            string ResponseJson = "{\"PaymentStatus\" : \"" + Response + "\"}";
            return ResponseJson;
        }

        /** This is copied from the CreateResponse method in the Woz Interface class.
         *  However, that method was specific to the Woz implementation.
         */
        private MessageTransactionResponse CreateResponse(string ResponseJson, string Info, string Data, TransactionState State)
        {
            MessageTransactionResponse RVal = new MessageTransactionResponse()
            {
                ResponseJson = ResponseJson,
                InformationalMessage = Info,
                Data = Data,
                State = State
            };
            return RVal;
        }
    }
}
