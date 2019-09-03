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
        private readonly ISubscriberService _subscriberService;

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
            _subscriberService = services.GetService<ISubscriberService>();
            _hangfireService = hangfireService;
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
        }


        public bool ProcessOrder(ServiceOfferingOrderDto serviceOfferingOrderDto, Guid subscriberGuid, ref int statusCode, ref string msg)
        {

            // TODO JAB Add logging 
            // validate that a subscriber has been specified 
            if (serviceOfferingOrderDto.Subscriber == null || serviceOfferingOrderDto.Subscriber?.SubscriberGuid == null)
            {
                statusCode = 404;
                msg = "Invalid subscriber from web form";
                return false;

            }
            // validate that the subscriber passed from the front end is the same as that from the JTW - Not sure if this is 
            // necessary but better safe that sorry 
            if (serviceOfferingOrderDto.Subscriber?.SubscriberGuid.Value != subscriberGuid)
            {
                statusCode = 403;
                msg = "Subscriber does not coorelate";
                return false;
            }
            // todo JAB validate service offering
            if (serviceOfferingOrderDto.ServiceOffering == null || serviceOfferingOrderDto.ServiceOffering.ServiceOfferingGuid == null)
            {
                statusCode = 400;
                msg = "Service offering not specified";
                return false;
            }

            ServiceOffering serviceOffering = _repositoryWrapper.ServiceOfferingRepository.GetByGuid(serviceOfferingOrderDto.ServiceOffering.ServiceOfferingGuid);
            if (serviceOffering == null)
            {
                statusCode = 404;
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
                    return false;
                }
            }
            else
            {
                // Find promo code 
                PromoCode promoCode = _repositoryWrapper.PromoCodeRepository.GetByName(serviceOfferingOrderDto.PromoCode.PromoName);
                if (promoCode == null)
                {
                    statusCode = 400;
                    msg = $"{serviceOfferingOrderDto.PromoCode.PromoName} is not a valid promo code";
                    return false;
                }
                // Find service offering promo code object 
                List<ServiceOfferingPromoCode> serviceOfferingPromoCodes = _repositoryWrapper.ServiceOfferingPromoCodeRepository.GetByPromoCodesId(promoCode.PromoCodeId);
                if (serviceOfferingPromoCodes.Count <= 0 )                    
                {
                    statusCode = 400;
                    msg = $"{serviceOfferingOrderDto.PromoCode.PromoName} is not a valid for service offerings";
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
                    msg = $"{serviceOfferingOrderDto.PromoCode.PromoName} is not a valid for service {serviceOffering.Name}";
                    return false;
                }

                // TODO move to PromoCodeService 



                // Todo check promo expiration dates 
                // check max number of redemptions 
                // Make sure the numbers add up 



            }

      
        

            // todo JAB run order through braintree

            // todo JAB create ServiceOffering record

            // todo JAB create serviceoffering promo code redepmtion recor 

            // todo jab update serviceOfferingPromoCode number of redemptions 


            // return OK 



            return true;
        }

    }
}
