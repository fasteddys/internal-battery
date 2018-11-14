﻿using AutoMapper;
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
            SubscriberDto SubscriberDto = null;
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
                SubscriberDto = EnrollmentFlowDto.SubscriberDto;
            }
            catch (Exception e)
            {
                string ErrorMessage = "BraintreePaymentFlow:PaymentWorkItem -> No Braintree, subscriber or enrollment dto present; enrollment has been cancelled.";
                _sysLog.SysError(ErrorMessage);
                _sysEmail.SendEmail(ErrorsEmailAddress, "An error has occurred in the enrollment flow", CreateErrorEmail(ErrorMessage, EnrollmentFlowDto));
                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentError);
                return CreateResponse(CreateResponseJson(ErrorMessage), ErrorMessage, string.Empty, TransactionState.FatalError);
            }


            IBraintreeGateway gateway = _braintreeConfiguration.GetGateway();
            string nonce = BraintreePaymentDto.Nonce;
            TransactionRequest TransactionRequest = null;

            // Make sure we don't get null object exceptions when referencing DTOs


            // Make sure Nonce is present when there is a non-zero payment amount
            if (string.IsNullOrEmpty(nonce) && BraintreePaymentDto.PaymentAmount > 0)
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
                // the following variable exists to differentiate between a successful payment and a successful transaction. 
                // this is necessary because courses can be free with a promo code, in which case no payment is made. 
                // in this case, the transaction is still successful even though no payment was made.
                bool isTransactionSuccessful = false;

                Result<Transaction> paymentResult = null;

                if (BraintreePaymentDto.PaymentAmount > 0)
                {
                    paymentResult = gateway.Transaction.Sale(TransactionRequest);
                    Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentComplete);
                    _sysLog.SysInfo("BraintreePaymentFlow:PaymentWorkItem -> Braintree payment in progress.");
                    if (paymentResult.IsSuccess())
                        isTransactionSuccessful = true;
                }
                else
                {
                    // payment amount was zero, therefore we do not attempt to process a payment. 
                    // transaction is considered to be successful.
                    isTransactionSuccessful = true;
                }

                if(isTransactionSuccessful)
                {
                    string SuccessfulMessage = "BraintreePaymentFlow:PaymentWorkItem->Braintree payment was successful.";
                    _sysLog.SysInfo(SuccessfulMessage);
                    Course course = _db.Course.Where(t => t.IsDeleted == 0 && t.CourseId == EnrollmentDto.CourseId).FirstOrDefault();
                    EnrollmentLog enrollmentLog = _db.EnrollmentLog.Where(t => t.IsDeleted == 0 && t.EnrollmentGuid == EnrollmentDto.EnrollmentGuid).FirstOrDefault();
                    _sysEmail.SendPurchaseReceiptEmail(SubscriberDto.Email, "Purchase Receipt For CareerCircle Course", course.Name, enrollmentLog.CourseCost, enrollmentLog.PromoApplied, (Guid)EnrollmentDto.EnrollmentGuid);
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
            if (BraintreePaymentObject.StateGuid.HasValue)
            {
                State state = _db.State.Where(s => s.StateGuid == BraintreePaymentObject.StateGuid.Value).FirstOrDefault();
                if (state != null)
                {
                    BraintreePaymentObject.Region = state.Code;
                }
            }
            if (BraintreePaymentObject.CountryGuid.HasValue)
            {
                Country country = _db.Country.Where(c => c.CountryGuid == BraintreePaymentObject.CountryGuid.Value).FirstOrDefault();
                if (country != null)
                {
                    BraintreePaymentObject.CountryCode = country.Code2;
                }
            }

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
