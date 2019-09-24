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
    public class PromoCodeService : IPromoCodeService
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

        public PromoCodeService(IServiceProvider services, IHangfireService hangfireService)
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

        /// <summary>
        ///  Check to see if the given subscriber can redeem the given promo
        /// </summary>
        /// <param name="promoCode"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public bool SubscriberHasAvailableRedemptions(PromoCode promoCode, Subscriber subscriber)
        {

            // return true to ignore this check if the promo code subscriber is not specified 
            if (promoCode == null || subscriber == null || promoCode.MaxNumberOfRedemptionsPerSubscriber == null)
                return true;

            int numRedemptions = _db.ServiceOfferingPromoCodeRedemption
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriber.SubscriberId && s.PromoCodeId == promoCode.PromoCodeId && s.RedemptionStatusId == 2)
                .Count();

            if (numRedemptions >= promoCode.MaxNumberOfRedemptionsPerSubscriber)
                return false;
            else
                return true;
       
        }


        public decimal CalculatePrice(PromoCode promoCode, decimal BasePrice)
        {
            decimal rVal = BasePrice;
            // todo get rid of magic number somehow
            if (promoCode.PromoTypeId == 1)
                rVal = rVal - promoCode.PromoValueFactor;
            else if (promoCode.PromoTypeId == 2)
                rVal = rVal - (rVal * promoCode.PromoValueFactor);

            if (rVal < 0)
                rVal = 0;

            return rVal;
        }
        public bool ValidateStartDate(PromoCode promoCode)
        {
            if (promoCode.PromoStartDate != null && promoCode.PromoStartDate != DateTime.MinValue && promoCode.PromoStartDate > DateTime.UtcNow)            
                return false;            
            else
                return true;
        }
        public bool ValidateEndDate(PromoCode promoCode)
        {
            if (promoCode.PromoEndDate != null && promoCode.PromoEndDate != DateTime.MinValue && promoCode.PromoEndDate < DateTime.UtcNow)
                return false;
            else
                return true;
        }

        public bool ValidateRedemptions(PromoCode promoCode)
        {
            if (promoCode.MaxAllowedNumberOfRedemptions != 0 && promoCode.NumberOfRedemptions > promoCode.MaxAllowedNumberOfRedemptions)
                return false;
            else
                return true;
        }



    }
}
