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
using UpDiddyApi.ApplicationCore.Factory;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ServiceOfferingOrderService : IServiceOfferingOrderService
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
        private IBraintreeService _braintreeService;
        private readonly IServiceOfferingPromoCodeRedemptionService _serviceOfferingPromoCodeRedemptionService;

        public ServiceOfferingOrderService(IServiceProvider services, IHangfireService hangfireService)
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
            _braintreeService = services.GetService<IBraintreeService>();
            _serviceOfferingPromoCodeRedemptionService = services.GetService<IServiceOfferingPromoCodeRedemptionService>();

            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
        }

   

        public bool ProcessOrder(ServiceOfferingTransactionDto serviceOfferingTransactionDto, Guid subscriberGuid, ref int statusCode, ref string msg)
        {

            _syslog.LogInformation("ServiceOfferingService.ProcessOrder starting", serviceOfferingTransactionDto);
            ServiceOfferingOrderDto serviceOfferingOrderDto = serviceOfferingTransactionDto.ServiceOfferingOrderDto;
            Subscriber subscriber = null;
            PromoCode promoCode = null;
            ServiceOffering serviceOffering = null;
            // Validate basic aspects of the transaction such a valid service offering, valid promo etc. 
            if (ValidateTransaction(serviceOfferingOrderDto, ref serviceOffering, ref promoCode, ref statusCode, ref msg) == false)
                return false;

            if (ValidateSubscriber(serviceOfferingTransactionDto, subscriberGuid, ref subscriber, ref statusCode, ref msg) == false)
                return false;

            //
            // At this point, subscriber should by hydrated with an existing or newly created subscriber.
            //

            // Validate subscriber constraints such as max number of redemptions per subscriber
            if (ValidateSubscriberConstraints(promoCode, subscriber, ref statusCode, ref msg) == false)
                return false;

            // validate the payment with braintree
            if (ValidatePayment(serviceOfferingTransactionDto, subscriberGuid, ref subscriber, ref statusCode, ref msg) == false)
                return false;

       
            // create service offering order record 
            ServiceOfferingOrder order = new ServiceOfferingOrder()
            {
                CreateDate = DateTime.Now,
                CreateGuid = subscriber.SubscriberGuid.Value,
                IsDeleted = 0,
                ModifyDate = DateTime.Now,
                ModifyGuid = subscriber.SubscriberGuid.Value,
                PercentCommplete = 0,
                PricePaid = serviceOfferingOrderDto.PricePaid,
                SubscriberId = subscriber.SubscriberId,
                ServiceOfferingId = serviceOffering.ServiceOfferingId,
                ServiceOfferingOrderGuid = Guid.NewGuid()
            };



            string SubscriberEmail = subscriber.Email;
            decimal PromoDiscount = serviceOfferingTransactionDto.ServiceOfferingOrderDto.PromoCode?.Discount != null ? 
                serviceOfferingTransactionDto.ServiceOfferingOrderDto.PromoCode.Discount : 0;
            decimal FinalCost = serviceOfferingTransactionDto.ServiceOfferingOrderDto.PromoCode?.FinalCost != null ? 
                serviceOfferingTransactionDto.ServiceOfferingOrderDto.PromoCode.FinalCost : 
                serviceOfferingTransactionDto.ServiceOfferingOrderDto.ServiceOffering.Price;


            _hangfireService.Enqueue(() => _sysEmail.SendTemplatedEmailAsync(
                SubscriberEmail,
                _configuration["SysEmail:Transactional:TemplateIds:PurchaseReceipt-CareerServices"],
                new
                {
                    packageName = serviceOfferingTransactionDto.ServiceOfferingOrderDto.ServiceOffering.Name,
                    packagePrice = "$" + serviceOfferingTransactionDto.ServiceOfferingOrderDto.ServiceOffering.Price.ToString(),
                    promoDiscount = "$" + PromoDiscount.ToString(),
                    pricePaid = "$" + FinalCost,
                    purchaseGuid = order.ServiceOfferingOrderGuid
                },
               Constants.SendGridAccount.Transactional,
               null,
               null,
               null,
               null
                ));

            if (promoCode != null)
            {
                order.PromoCodeId = promoCode.PromoCodeId;
                //create or convert in flight promo code redemption row
                _serviceOfferingPromoCodeRedemptionService.ClaimServiceOfferingPromoCode(promoCode, serviceOffering, subscriber, order.PricePaid);                
                // increment the number of redemptions of the coupon.
                // todo jab makse sure ef catches this update                
                promoCode.ModifyDate = DateTime.Now;
                promoCode.NumberOfRedemptions += 1;
            }
            
            _db.ServiceOfferingOrder.Add(order);
            _db.SaveChanges();
            _syslog.LogInformation("ServiceOfferingService.ProcessOrder finished returning true", serviceOfferingTransactionDto);
            msg = order.ServiceOfferingOrderGuid.ToString();
            return true;

        }

        public bool ValidateSubscriberConstraints(PromoCode promoCode, Subscriber subscriber, ref int statusCode, ref string msg)
        {
            if (_promoCodeService.CheckSubscriberRedemptions(promoCode, subscriber) == false)
            {
                msg = "Sorry you have already redeemed this offer";
                statusCode = 400;
                _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriberConstraints returning false: {msg} ");
                return false;
            }
                
            return true;
        }

            public bool ValidatePayment(ServiceOfferingTransactionDto serviceOfferingTransactionDto, Guid subscriberGuid, ref Subscriber subscriber, ref int statusCode, ref string msg)
        {
            _syslog.LogInformation("ServiceOfferingService.ValidatePayment starting", serviceOfferingTransactionDto);
            ServiceOfferingOrderDto serviceOfferingOrderDto = serviceOfferingTransactionDto.ServiceOfferingOrderDto;
            if ( serviceOfferingOrderDto.PricePaid > 0 )
            {
                if (serviceOfferingTransactionDto.BraintreePaymentDto == null )
                {
                    msg = "Braintree payment information is missing";
                    statusCode = 400;
                    _syslog.LogInformation($"ServiceOfferingService.ValidatePayment returning false: {msg} ");
                    return false;
                }
                // call braintree to capture payment 
                if (_braintreeService.CapturePayment(serviceOfferingTransactionDto.BraintreePaymentDto, ref statusCode, ref msg) == false)
                {
                    _syslog.LogInformation($"ServiceOfferingService.ValidatePayment returning false: {msg} ");
                    return false;
                }
                    
                
            }

            _syslog.LogInformation("ServiceOfferingService.ValidatePayment finished returning trie");
            return true;
        }



        /// <summary>
        /// Validate the subscriber.  This method MUST return either a valid subscriber or return false since other code assumes it does.
        /// </summary>
        /// <param name="serviceOfferingTransactionDto"></param>
        /// <param name="subscriberGuid"></param>
        /// <param name="subscriber"></param>
        /// <param name="statusCode"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ValidateSubscriber(ServiceOfferingTransactionDto serviceOfferingTransactionDto, Guid subscriberGuid, ref Subscriber subscriber, ref int statusCode, ref string msg)
        {
            _syslog.LogInformation("ServiceOfferingService.ValidateSubscriber starting", serviceOfferingTransactionDto);

            ServiceOfferingOrderDto serviceOfferingOrderDto = serviceOfferingTransactionDto.ServiceOfferingOrderDto;

            // validate that the subscriber is logged in 
            if ( subscriberGuid != Guid.Empty)
            {

                // validate that a subscriber has been specified by the front end 
                if (serviceOfferingOrderDto.Subscriber == null || serviceOfferingOrderDto.Subscriber?.SubscriberGuid == null)
                {
                    statusCode = 404;
                    msg = "For authenticated requests, subscriber must be supplied by front-en";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    return false;

                }

                // validate that the subscriber passed from the front end is the same as that from the JTW - Not sure if this is 
                // necessary but better safe that sorry 
                if (serviceOfferingOrderDto.Subscriber?.SubscriberGuid.Value != subscriberGuid)
                {
                    statusCode = 403;
                    msg = "Requesting subscriber does not match logged in subscriber";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    return false;
                }

                subscriber = _repositoryWrapper.SubscriberRepository.GetSubscriberByGuid(subscriberGuid);

                if (subscriber == null)
                {
                    statusCode = 404;
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    msg = "Unable to locate subscriber specified by JWT";
                    return false;
                }


            }
            else
            {             
                //todo move to subscriber service 
                if (serviceOfferingTransactionDto.SignUpDto == null )
                {
                    statusCode = 403;
                    msg = "Signup information required for non-authenticated requests.";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    return false;
                }

                
                // validate email
                if ( string.IsNullOrEmpty(serviceOfferingTransactionDto.SignUpDto.email) || Utils.ValidateEmail(serviceOfferingTransactionDto.SignUpDto.email) == false  )
                {
                    statusCode = 400;
                    msg = $"'{serviceOfferingTransactionDto.SignUpDto.email}' is an invalid signup email";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    return false;
                }

                // validate password 
                // todo enforce better password complexity - it appears that /api/[controller]/express-sign-up relies on the front-end validation 
                if (string.IsNullOrEmpty(serviceOfferingTransactionDto.SignUpDto.password) )
                {
                    statusCode = 400;
                    msg = $"Signup password is required";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    return false;
                }


                // validate the email is not an existing subscriber 
                Subscriber existingSubscriber = _repositoryWrapper.SubscriberRepository.GetSubscriberByEmail(serviceOfferingTransactionDto.SignUpDto.email);

                if ( existingSubscriber != null)
                {
                    statusCode = 400;
                    msg = $"'{serviceOfferingTransactionDto.SignUpDto.email}' is an existing subscriber";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    return false;
                }


                // check if user exits in AD if the user does then we skip this step
                Microsoft.Graph.User user = _graphClient.GetUserBySignInEmail(serviceOfferingTransactionDto.SignUpDto.email).Result;
 
                if (user == null)
                {
                    try
                    {
                        user =  _graphClient.CreateUser(serviceOfferingTransactionDto.SignUpDto.email, serviceOfferingTransactionDto.SignUpDto.email, serviceOfferingTransactionDto.SignUpDto.password).Result;
           
                    }
                    catch (Exception ex)
                    {
                        statusCode = 400;
                        msg = $"Error creating account: {ex.Message}";
                        _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                        return false;
                    }
                }

                // create subscriber for user
                subscriber = new Subscriber();
                subscriber.SubscriberGuid = Guid.Parse(user.AdditionalData["objectId"].ToString());
                subscriber.Email = serviceOfferingTransactionDto.SignUpDto.email;
                subscriber.CreateDate = DateTime.UtcNow;
                subscriber.ModifyDate = DateTime.UtcNow;
                subscriber.IsDeleted = 0;
                subscriber.ModifyGuid = Guid.Empty;
                subscriber.CreateGuid = Guid.Empty;
                subscriber.IsVerified = false;
                _db.Add(subscriber);
                _db.SaveChanges();

                //TODO JAB send welcome email.  

                // load the newly created subscriber 
                 existingSubscriber = _repositoryWrapper.SubscriberRepository.GetSubscriberByEmail(serviceOfferingTransactionDto.SignUpDto.email);
                if (existingSubscriber == null)
                {
                    statusCode = 400;
                    msg = $"Unable to locate newly created subscriber with email '{serviceOfferingTransactionDto.SignUpDto.email}'";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateSubscriber returning false: {msg} ");
                    return false;
                }

                int tokenTtlMinutes = int.Parse(_configuration["EmailVerification:TokenExpirationInMinutes"]);
                EmailVerification.SetSubscriberEmailVerification(existingSubscriber, tokenTtlMinutes);

                _hangfireService.Enqueue(() =>
                _sysEmail.SendTemplatedEmailAsync(
                    serviceOfferingTransactionDto.SignUpDto.email,
                    _configuration["SysEmail:Transactional:TemplateIds:EmailVerification-LinkEmail"],
                    new
                    {
                        verificationLink = serviceOfferingTransactionDto.SignUpDto.verifyUrl + existingSubscriber.EmailVerification.Token
                    },
                    Constants.SendGridAccount.Transactional,
                    null,
                    null,
                    null,
                    null
                ));
            }

            _syslog.LogInformation("ServiceOfferingService.ValidateSubscriber finished returning true");
            return true;

        }


    
        /// <summary>
        /// This function must either return a serviceOffering object or an error.  Other code assumes this
        /// to be the case.
        /// </summary>
        /// <param name="serviceOfferingOrderDto"></param>
        /// <param name="serviceOffering"></param>
        /// <param name="promoCode"></param>
        /// <param name="statusCode"></param>
        /// <param name="msg"></param>
        /// <returns></returns>



     
        public bool ValidateTransaction(ServiceOfferingOrderDto serviceOfferingOrderDto, ref ServiceOffering serviceOffering, ref PromoCode promoCode, ref int statusCode, ref string msg)
        {
            _syslog.LogInformation("ServiceOfferingService.ValidateTransaction starting", serviceOfferingOrderDto);

            // validate that a subscriber has been specified 
            if (serviceOfferingOrderDto == null )
            {
                statusCode = 400;
                _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                msg = "Service offering order not found";
                return false;
            }

            // validate service offering
            if (serviceOfferingOrderDto.ServiceOffering == null || serviceOfferingOrderDto.ServiceOffering.ServiceOfferingGuid == null)
            {
                statusCode = 400;
                _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                msg = "Service offering not specified";
                return false;
            }

            serviceOffering = _repositoryWrapper.ServiceOfferingRepository.GetByGuid(serviceOfferingOrderDto.ServiceOffering.ServiceOfferingGuid);
            if (serviceOffering == null)
            {
                statusCode = 404;
                _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                msg = "Service offering not found";
                return false;
            }

            // validate case of no promo code 
            if (serviceOfferingOrderDto.PromoCode == null ||  string.IsNullOrEmpty(serviceOfferingOrderDto.PromoCode.PromoName ) )
            {
                if ( serviceOfferingOrderDto.PricePaid != serviceOffering.Price)
                {
                    statusCode = 400;
                    msg = $"Passed price of {serviceOfferingOrderDto.PricePaid} does not match system price of {serviceOffering.Price}";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;
                }
            }
            else
            {
                // Find promo code 
                promoCode = _repositoryWrapper.PromoCodeRepository.GetByName(serviceOfferingOrderDto.PromoCode.PromoName);
                if (promoCode == null)
                {
                    statusCode = 400;
                    msg = $"Promo code {serviceOfferingOrderDto.PromoCode.PromoName} is not a valid promo code";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;
                }

                // Validate the the promo start date
                if (_promoCodeService.ValidateStartDate(promoCode) == false)
                {
                    statusCode = 400;
                    msg = $"Promo code {serviceOfferingOrderDto.PromoCode.PromoName} does not start until {promoCode.PromoStartDate.ToShortDateString()}";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;
                }

                // Validate the the promo end date
                if ( _promoCodeService.ValidateEndDate(promoCode) == false )
                {
                    statusCode = 400;
                    msg = $"Promo code {serviceOfferingOrderDto.PromoCode.PromoName} ended on {promoCode.PromoEndDate.ToShortDateString()}";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;
                }


                // TODO move to PromoCodeService 

                // check max number of redemptions 
                if ( _serviceOfferingPromoCodeRedemptionService.CheckAvailability(promoCode, serviceOffering ) == false )
                {
                    statusCode = 400;
                    msg = $"Promo code {serviceOfferingOrderDto.PromoCode.PromoName} has exceeded it's redemption limit";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;
                }


                // Find service offering promo code object 
                List<ServiceOfferingPromoCode> serviceOfferingPromoCodes = _repositoryWrapper.ServiceOfferingPromoCodeRepository.GetByPromoCodesId(promoCode.PromoCodeId);
                if (serviceOfferingPromoCodes.Count <= 0 )                    
                {
                    statusCode = 400;
                    msg = $"Promo code {serviceOfferingOrderDto.PromoCode.PromoName} is not a valid for service offerings";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;
                }

                // TODO move to ServiceOfferingPromoCodeService 
                // Validate that the promo code is valid for the purchased service offering
                bool isPromoValid = false;
                foreach (ServiceOfferingPromoCode s in serviceOfferingPromoCodes)
                {
                    if ( s.ServiceOfferingId == -1 || s.ServiceOfferingId == serviceOffering.ServiceOfferingId)
                    {
                        isPromoValid = true;
                        break;
                    }

                }
                if (isPromoValid == false)
                {
                    statusCode = 400;
                    msg = $"Promo code {serviceOfferingOrderDto.PromoCode.PromoName} is not a valid for service {serviceOffering.Name}";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;
                }

                // Make sure the numbers add up 
                decimal adjustedPrice = _promoCodeService.CalculatePrice(promoCode, serviceOffering.Price);
                if ( adjustedPrice != serviceOfferingOrderDto.PricePaid)
                {
                    statusCode = 400;
                    msg = $"The price paid {serviceOfferingOrderDto.PricePaid} does not match the calulated promo price of {adjustedPrice}";
                    _syslog.LogInformation($"ServiceOfferingService.ValidateTransaction returning false: {msg} ");
                    return false;


                }
           
            }

            _syslog.LogInformation("ServiceOfferingService.ValidateTransaction finished returning true");
            return true;
        }
        
        public async Task<ServiceOfferingOrderDto> GetSubscriberOrder(Guid ServiceOfferingOrderGuid){
            ServiceOfferingOrder serviceOfferingOrder = await _repositoryWrapper.ServiceOfferingOrderRepository.GetByGuidAsync(ServiceOfferingOrderGuid);
            ServiceOfferingOrderDto serviceOfferingOrderDto = _mapper.Map<ServiceOfferingOrderDto>(serviceOfferingOrder);
            return serviceOfferingOrderDto;
        }
    }
}
