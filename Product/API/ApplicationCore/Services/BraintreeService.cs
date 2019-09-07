using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Helpers.Job;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Shared.GoogleJobs;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Shared;
using Braintree;
using UpDiddyLib.Helpers.Braintree;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class BraintreeService : IBraintreeService
    {
        private readonly IServiceProvider _services;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private ISysEmail _sysEmail;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private IHangfireService _hangfireService;
        private readonly CloudTalent _cloudTalent = null;
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ICompanyService _companyService;
        private readonly IPromoCodeService _promoCodeService;
        private readonly ISubscriberService _subscriberService;
        private IB2CGraph _graphClient;
        private IBraintreeConfiguration _braintreeConfiguration = null;

        public BraintreeService(IServiceProvider services, IHangfireService hangfireService)
        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _syslog = _services.GetService<ILogger<JobService>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _mapper = _services.GetService<IMapper>();
            _sysEmail = _services.GetService<ISysEmail>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _companyService = services.GetService<ICompanyService>();
            _promoCodeService = services.GetService<IPromoCodeService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _hangfireService = hangfireService;
            _graphClient = services.GetService<IB2CGraph>();
            _braintreeConfiguration = new BraintreeConfiguration(_configuration);
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
        }

        public bool CapturePayment(BraintreePaymentDto braintreePaymentDto, ref string authID, ref int statusCode, ref string msg)
        {

            _syslog.LogInformation("BraintreeService.CapturePayment starting", braintreePaymentDto);
            IBraintreeGateway gateway = _braintreeConfiguration.GetGateway();
            string nonce = braintreePaymentDto.Nonce;
            TransactionRequest TransactionRequest = null;


            // Make sure Nonce is present when there is a non-zero payment amount
            if (string.IsNullOrEmpty(nonce) && braintreePaymentDto.PaymentAmount > 0)
            {
                msg = "BraintreePaymentFlow:PaymentWorkItem -> Braintree nonce is null or empty; enrollment has been cancelled.";
                statusCode = 400;
                _syslog.LogInformation($"BraintreeService.CapturePayment returning false: {msg} ");
                return false;
            }

            // Create the transaction request object
            try
            {
                TransactionRequest = AssembleTransactionRequest(braintreePaymentDto);
            }
            catch (Exception e)
            {
                msg = "BraintreePaymentFlow:PaymentWorkItem -> An error occurred when attempting to create the TransactionRequest.";
                statusCode = 400;
                _syslog.LogInformation($"BraintreeService.CapturePayment returning false: {msg} Exception = {e.Message} ");
                return false;                
            }

            // payment amount was zero, therefore we do not attempt to process a payment. 
            // transaction is considered to be successful.
            if (braintreePaymentDto.PaymentAmount <= 0)
            {
                _syslog.LogInformation("BraintreeService.CapturePayment finished payment amount <= 0 returning true");
                return true;
            }
                
            try
            {
                // the following variable exists to differentiate between a successful payment and a successful transaction. 
                // this is necessary because courses can be free with a promo code, in which case no payment is made. 
                // in this case, the transaction is still successful even though no payment was made.
                bool isTransactionSuccessful = false;

                Result<Transaction> paymentResult = gateway.Transaction.Sale(TransactionRequest);
                if (paymentResult.IsSuccess())
                {
                    try
                    {
                        authID = "transactionId=" + paymentResult.Target.NetworkTransactionId + ";authorization=" + paymentResult.Target.AuthorizedTransactionId;
                    }
                    catch (Exception ex)
                    {
                        authID = "Error: " + ex.Message;
                    }
                    return true;
                }                    
                else
                {
                    string braintreeErrors = string.Empty;
                    if (paymentResult.Message == "Gateway Rejected: avs")
                    {
                        msg = $"Billing address is incorrect, please confirm your street address and  postal code";
                    }
                    else if (paymentResult.Message == "Gateway Rejected: cvv")
                    {
                        msg = $"Your cvv code is incorrect";
                    }
                    else
                    {
                        List<ValidationError> theErrors = paymentResult.Errors.DeepAll();

                        foreach (ValidationError ve in theErrors)
                            braintreeErrors += ve.Message + ";";
                        msg = $"Braintree capture failed: Message {paymentResult.Message} Braintree errors: {braintreeErrors}";
                    }
                    statusCode = 400;
                    _syslog.LogInformation($"BraintreeService.CapturePayment returning false: {msg} ");
                    return false;
                }
                       
            }
            catch (Exception ex)
            {
                msg = $"Braintree capture failed: {ex.Message}";
                statusCode = 500;
                _syslog.LogInformation($"BraintreeService.CapturePayment returning false: {msg} ");
                return false;
            }


            _syslog.LogInformation("BraintreeService.CapturePayment finished returning true");
            return true;
        }
        #region Private Helper Functions 

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
                 //   PostalCode = BraintreePaymentObject.ZipCode,
                    CountryCodeAlpha2 = BraintreePaymentObject.CountryCode
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
            };
            return request;
        }

        #endregion


    }
}
