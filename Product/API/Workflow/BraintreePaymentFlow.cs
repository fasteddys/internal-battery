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
        private int _retrySeconds = 0;
        private int _wozVendorId = 0;
        private IBraintreeConfiguration braintreeConfiguration;

        public BraintreePaymentFlow(UpDiddyDbContext dbcontext, IMapper mapper, IConfiguration configuration, ISysEmail sysEmail, ISysLog sysLog)
        {
            _retrySeconds = int.Parse(configuration["Woz:RetrySeconds"]);
            // TODO modify code to work off woz Guid not dumb key 
            _wozVendorId = int.Parse(configuration["Woz:VendorId"]);
            _db = dbcontext;
            _mapper = mapper;
            _configuration = configuration;
            _sysEmail = sysEmail;
            _sysLog = sysLog;
            braintreeConfiguration = new BraintreeConfiguration(_configuration);
        }

        public async Task<MessageTransactionResponse> PaymentWorkItem(EnrollmentFlowDto EnrollmentFlowDto)
        {
            // TODO: Wire up logging
            MessageTransactionResponse RVal = null;

            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);
            BraintreePaymentDto BraintreePaymentDto = EnrollmentFlowDto.BraintreePaymentDto;
            EnrollmentDto EnrollmentDto = EnrollmentFlowDto.EnrollmentDto;
            var gateway = braintreeConfiguration.GetGateway();
            var nonce = BraintreePaymentDto.Nonce;
            AddressRequest addressRequest;


            addressRequest = new AddressRequest
            {
                // Assuming form fields are filled in at this point until above TODO is handled
                FirstName = BraintreePaymentDto.FirstName,
                LastName = BraintreePaymentDto.LastName,
                StreetAddress = BraintreePaymentDto.Address,
                Region = BraintreePaymentDto.Region,
                Locality = BraintreePaymentDto.Locality,
                PostalCode = BraintreePaymentDto.ZipCode,
                CountryCodeAlpha2 = BraintreePaymentDto.CountryCode

            };

            TransactionRequest request = new TransactionRequest
            {
                Amount = BraintreePaymentDto.PaymentAmount,
                MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("BraintreeMerchantAccountID"),
                PaymentMethodNonce = nonce,
                Customer = new CustomerRequest
                {
                    FirstName = BraintreePaymentDto.FirstName,
                    LastName = BraintreePaymentDto.LastName,
                    Phone = BraintreePaymentDto.PhoneNumber,
                    Email = BraintreePaymentDto.Email
                },
                BillingAddress = addressRequest,
                ShippingAddress = new AddressRequest
                {
                    FirstName = BraintreePaymentDto.FirstName,
                    LastName = BraintreePaymentDto.LastName,
                    StreetAddress = BraintreePaymentDto.Address
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
            };


            try
            {
                Result<Transaction> result = gateway.Transaction.Sale(request);
                Helper.UpdateEnrollmentStatus(EnrollmentFlowDto.EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentInitiated);

                if (result.IsSuccess())
                {
                    // TODO: Log successful braintree payment
                    Helper.UpdateEnrollmentStatus(EnrollmentFlowDto.EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentComplete);
                    BackgroundJob.Enqueue<WozEnrollmentFlow>(x => x.EnrollStudentWorkItem(EnrollmentFlowDto.EnrollmentDto.EnrollmentGuid.ToString()));
                }
                else
                {
                    // TODO: Log failed braintree payment
                    Helper.UpdateEnrollmentStatus(EnrollmentFlowDto.EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentFatalError);
                }

            }
            catch (Exception ex)
            {
                // Log 
            }
            return RVal;
        }
    }
}
