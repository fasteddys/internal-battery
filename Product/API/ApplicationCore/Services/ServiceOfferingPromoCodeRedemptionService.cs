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

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ServiceOfferingPromoCodeRedemptionService : IServiceOfferingPromoCodeRedemptionService
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

        public ServiceOfferingPromoCodeRedemptionService(IServiceProvider services, IHangfireService hangfireService)
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
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
        }

        public bool ReserveServiceOfferingPromoCode(PromoCode promoCode, ServiceOffering serviceOffering, Subscriber subscriber, decimal adjustedPrice)
        {
            //TODO JAB - add code to purge orphaned remptions here orphaned reserved 

            // delete all existing in flight promos for the user 
            List<ServiceOfferingPromoCodeRedemption> existingPromos = _db.ServiceOfferingPromoCodeRedemption
                    .Where(s => s.IsDeleted == 0 && s.RedemptionStatusId == 2 && s.ServiceOfferingId == serviceOffering.ServiceOfferingId && s.SubscriberId == subscriber.SubscriberId)
                    .ToList();

            foreach (ServiceOfferingPromoCodeRedemption s in existingPromos)
                s.IsDeleted = 1;

            // create new promo redemtion 
            ServiceOfferingPromoCodeRedemption redemption = new ServiceOfferingPromoCodeRedemption()
            {
                CreateDate = DateTime.UtcNow,
                CreateGuid = subscriber.SubscriberGuid.Value,
                IsDeleted = 0,
                ModifyDate = DateTime.UtcNow,
                ModifyGuid = subscriber.SubscriberGuid.Value,
                SubscriberId = subscriber.SubscriberId,
                PromoCodeId = promoCode.PromoCodeId,
                ServiceOfferingId = serviceOffering.ServiceOfferingId,
                RedemptionDate = DateTime.UtcNow,
                ValueRedeemed = adjustedPrice,
                // todo make this an enum since having a foreign key to a table that only contains status information
                // is overly complicated
                RedemptionStatusId = 1 // inflight 
            };
            _db.ServiceOfferingPromoCodeRedemption.Add(redemption);
            _db.SaveChanges();

            return true;

        }

        public bool ClaimServiceOfferingPromoCode(PromoCode promoCode, ServiceOffering serviceOffering, Subscriber subscriber, decimal adjustedPrice)
        {
            // find existing inflight promo code for user in flip it 
            ServiceOfferingPromoCodeRedemption reservedPromo = _db.ServiceOfferingPromoCodeRedemption
                .Where(s => s.IsDeleted == 0 && s.RedemptionStatusId == 1 && s.ServiceOfferingId == serviceOffering.ServiceOfferingId && s.SubscriberId == subscriber.SubscriberId)
                .FirstOrDefault();

            if (reservedPromo != null)
                reservedPromo.RedemptionStatusId = 2; // complete existing 
            else
            {
                ServiceOfferingPromoCodeRedemption redemption = new ServiceOfferingPromoCodeRedemption()
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = subscriber.SubscriberGuid.Value,
                    IsDeleted = 0,
                    ModifyDate = DateTime.UtcNow,
                    ModifyGuid = subscriber.SubscriberGuid.Value,
                    SubscriberId = subscriber.SubscriberId,
                    PromoCodeId = promoCode.PromoCodeId,
                    ServiceOfferingId = serviceOffering.ServiceOfferingId,
                    RedemptionDate = DateTime.UtcNow,
                    ValueRedeemed = adjustedPrice,
                    // todo make this an enum since having a foreign key to a table that only contains status information
                    // is overly complicated
                    RedemptionStatusId = 2 // Complete 
                };
                _db.ServiceOfferingPromoCodeRedemption.Add(redemption);
            }            
            _db.SaveChanges();

            return true;
        }

        public bool CheckAvailability(PromoCode promoCode, ServiceOffering serviceOffering)
        {
            // get the number of redemtions (either completed or inflight )
            int NumRedemptions = _db.ServiceOfferingPromoCodeRedemption
                .Where(s => s.IsDeleted == 0 && s.ServiceOfferingId == s.ServiceOfferingId && s.PromoCodeId == promoCode.PromoCodeId)
                .Count();

            if (NumRedemptions >= promoCode.NumberOfRedemptions)
                return false;


            return true;
        }




    }
}
