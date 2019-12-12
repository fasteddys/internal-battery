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
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore.Exceptions;

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
        private readonly ICloudTalentService _cloudTalentService;
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ICompanyService _companyService;
        private readonly ISubscriberService _subscriberService;

        public PromoCodeService(IServiceProvider services, IHangfireService hangfireService, ICloudTalentService cloudTalentService)
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
            _cloudTalentService = cloudTalentService;
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


        public bool Redeem(Guid subscriberGuid, Guid promoCodeRedemptionGuid, Guid courseVariantGuid)
        {
            bool rVal = false;
            // lookup redemption by guid and ensure that the following is true: 
            //  the promo code is still in process
            //  it is being redeemed for the same course and subscriber
            //  we will not exceed the number of allowed redemptions for this code
            var query = (from pcr in _db.PromoCodeRedemption.Include(pcr => pcr.RedemptionStatus)
                         join cv in _db.CourseVariant on pcr.CourseVariantId equals cv.CourseVariantId
                         join s in _db.Subscriber on pcr.SubscriberId equals s.SubscriberId
                         join pc in _db.PromoCode on pcr.PromoCodeId equals pc.PromoCodeId
                         where pcr.RedemptionStatus.Name == "In Process"
                         && pcr.IsDeleted == 0
                         && cv.CourseVariantGuid == courseVariantGuid
                         && s.SubscriberGuid == subscriberGuid
                         && s.IsDeleted == 0
                         && pcr.PromoCodeRedemptionGuid == promoCodeRedemptionGuid
                         && pc.NumberOfRedemptions < pc.MaxAllowedNumberOfRedemptions
                         select new PromoCodeDto()
                         {
                             Discount = pcr.ValueRedeemed,
                             IsValid = true,
                             PromoCodeRedemptionGuid = pcr.PromoCodeRedemptionGuid
                         }).FirstOrDefault();

            if (query != null)
            {
                rVal = true;
                // mark the promo code as completed
                var completedPromoCodeRedemption = _db.PromoCodeRedemption.Where(pcr => pcr.PromoCodeRedemptionGuid == query.PromoCodeRedemptionGuid).FirstOrDefault();
                completedPromoCodeRedemption.ModifyDate = DateTime.UtcNow;
                completedPromoCodeRedemption.ModifyGuid = Guid.Empty;
                completedPromoCodeRedemption.RedemptionStatusId = 2; // completed
                _db.Attach<PromoCodeRedemption>(completedPromoCodeRedemption);

                // increment the number of redemptions for the code
                var promoCodeId = (from pc in _db.PromoCode
                                   join pcr in _db.PromoCodeRedemption on pc.PromoCodeId equals pcr.PromoCodeId
                                   where pcr.PromoCodeRedemptionGuid == promoCodeRedemptionGuid
                                   select pc.PromoCodeId).FirstOrDefault();

                var promoCodeToUpdate = _db.PromoCode.Where(pc => pc.PromoCodeId == promoCodeId).FirstOrDefault();
                promoCodeToUpdate.ModifyDate = DateTime.UtcNow;
                promoCodeToUpdate.ModifyGuid = Guid.Empty;
                promoCodeToUpdate.NumberOfRedemptions += 1;
                _db.Attach<PromoCode>(promoCodeToUpdate);

                _db.SaveChanges();
            }

            return rVal;
        }




        public PromoCodeDto GetPromoCode(Guid subscriberGuid, string code, Guid courseVariantGuid)
        {
            PromoCodeDto validPromoCodeDto = null;

            #region business logic to refactor

            /*  todo: refactor this code. move business rules to IValidatableObject in Dto? to do this, i think we would need to
            *   use a custom value resolver in automapper to transform 5 model objects (PromoCode, Course, CoursePromoCode, 
            *   VendorPromoCode, SubscriberPromoCode) into a PromoCodeDto. once all of the necessary properties exist within that 
            *   Dto, then we could move all of the validation to:
            *       IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            */
            PromoCode promoCode = _db.PromoCode
                .Include(p => p.PromoType)
                .Where(p => p.Code == code)
                .FirstOrDefault();

            if (promoCode == null)
                throw new NotFoundException ("This promo code does not exist.");

            if (promoCode.IsDeleted == 1)
               throw new NotFoundException ("This promo code is no longer valid.");

            DateTime currentDateTime = DateTime.UtcNow;
            if (promoCode.PromoStartDate > currentDateTime.AddHours(-4)) // todo: improve ghetto grace period date logic
                throw new FailedValidationException("This promo code is not yet active." );

            if (promoCode.PromoEndDate < currentDateTime.AddHours(4)) // todo: improve ghetto grace period date logic
                throw new FailedValidationException("This promo code has expired.");

            var inProgressRedemptionsForThisCode = _db.PromoCodeRedemption
                .Include(pcr => pcr.RedemptionStatus)
                .Where(pcr => pcr.PromoCodeId == promoCode.PromoCodeId && pcr.IsDeleted == 0 && pcr.RedemptionStatus.Name == "In Process")
                .Count();

            if (promoCode.NumberOfRedemptions >= promoCode.MaxAllowedNumberOfRedemptions || promoCode.NumberOfRedemptions + inProgressRedemptionsForThisCode > promoCode.MaxAllowedNumberOfRedemptions)
                throw new FailedValidationException("This promo code has exceeded its allowed number of redemptions.");
 
            CourseVariant courseVariant = _db.CourseVariant
                .Where(cv => cv.CourseVariantGuid == courseVariantGuid)
                .FirstOrDefault();

            if (courseVariant == null)
                throw new NotFoundException("The course specified is invalid.");

            List<CourseVariantPromoCode> courseRestrictionsForThisPromoCode = _db.CourseVariantPromoCode
                .Where(cpc => cpc.PromoCodeId == promoCode.PromoCodeId && cpc.IsDeleted == 0)
                .ToList();

            if (courseRestrictionsForThisPromoCode.Any() && !courseRestrictionsForThisPromoCode.Any(r => r.CourseVariantId == courseVariant.CourseVariantId))
                throw new FailedValidationException("Promo code is not valid for this course.");
 

            Subscriber subscriber = _db.Subscriber
                .Where(s => s.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                .FirstOrDefault();

            if (subscriber == null)
                throw new FailedValidationException("The subscriber specified is invalid." );

            if (promoCode.MaxNumberOfRedemptionsPerSubscriber != null)
            {
                var completedSubscriberRedemptionsForThisCode = _db.PromoCodeRedemption
                .Include(pcr => pcr.RedemptionStatus)
                .Where(pcr => pcr.PromoCodeId == promoCode.PromoCodeId && pcr.IsDeleted == 0 && pcr.RedemptionStatus.Name == "Completed" && pcr.SubscriberId == subscriber.SubscriberId)
                .Count();

                if (completedSubscriberRedemptionsForThisCode >= promoCode.MaxNumberOfRedemptionsPerSubscriber)
                    return new PromoCodeDto()
                    {
                        IsValid = false,
                        ValidationMessage = "Your account has already redeemed this promo code the maximum number ("
                        + promoCode.MaxNumberOfRedemptionsPerSubscriber
                        + ") of times permitted. "
                    };
            }


            List<VendorPromoCode> vendorRestrictionsForThisPromoCode = _db.VendorPromoCode
                .Where(vpc => vpc.PromoCodeId == promoCode.PromoCodeId)
                .ToList();

            var course = _db.Course
                .Include(c => c.CourseVariants)
                .Where(c => c.CourseVariants
                    .Where(cv => cv.CourseVariantId == courseVariant.CourseVariantId)
                    .Any()
                ).FirstOrDefault();

            if (vendorRestrictionsForThisPromoCode.Any() && !vendorRestrictionsForThisPromoCode.Any(vpc => vpc.VendorId == course.VendorId))
                throw new FailedValidationException("Promo code is not valid for this vendor.");



            List<SubscriberPromoCode> subscriberRestrictionsForThisPromoCode = _db.SubscriberPromoCode
                .Where(spc => spc.PromoCodeId == promoCode.PromoCodeId)
                .ToList();

            if (subscriberRestrictionsForThisPromoCode.Any() && !subscriberRestrictionsForThisPromoCode.Any(spc => spc.SubscriberId == subscriber.SubscriberId))
                throw new FailedValidationException("Promo code is not valid for this subscriber." );

            #endregion

            // check if there is an existing "in process" promo code redemption for this code, course, and subscriber that has not been logically deleted
            var existingInProgressPromoCodeRedemption = _db.PromoCodeRedemption
                .Include(pcr => pcr.RedemptionStatus)
                .Where(pcr => pcr.PromoCodeId == promoCode.PromoCodeId && pcr.SubscriberId == subscriber.SubscriberId && pcr.CourseVariantId == courseVariant.CourseVariantId && pcr.RedemptionStatus.Name == "In Process" && pcr.IsDeleted == 0)
                .FirstOrDefault();

            if (existingInProgressPromoCodeRedemption == null)
            {
                // create a new promo code redemption and store it in the db
                validPromoCodeDto = _mapper.Map<PromoCodeDto>(promoCode);
                validPromoCodeDto.IsValid = true;
                validPromoCodeDto.ValidationMessage = $"The promo code '{promoCode.PromoName}' has been applied successfully! See below for the updated price.";
                switch (promoCode.PromoType.Name)
                {
                    case "Dollar Amount":
                        validPromoCodeDto.Discount = Math.Max(0, promoCode.PromoValueFactor);
                        break;
                    case "Percent Off":
                        validPromoCodeDto.Discount = Math.Max(0, Math.Round(courseVariant.Price * promoCode.PromoValueFactor, 2, MidpointRounding.ToEven));
                        break;
                    default:
                        throw new ApplicationException("Unrecognized promo type!");
                }
                validPromoCodeDto.FinalCost = courseVariant.Price - validPromoCodeDto.Discount;
                validPromoCodeDto.PromoCodeRedemptionGuid = Guid.NewGuid();
                _db.PromoCodeRedemption.Add(new PromoCodeRedemption()
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ModifyDate = DateTime.UtcNow,
                    ModifyGuid = Guid.Empty,
                    IsDeleted = 0,
                    PromoCodeRedemptionGuid = validPromoCodeDto.PromoCodeRedemptionGuid,
                    RedemptionStatusId = 1, // "In Process"
                    ValueRedeemed = validPromoCodeDto.Discount,
                    CourseVariantId = courseVariant.CourseVariantId,
                    PromoCodeId = promoCode.PromoCodeId,
                    SubscriberId = subscriber.SubscriberId
                });
                _db.SaveChanges();
            }
            else
            {
                // use the existing "in process" promo code redemption
                validPromoCodeDto = new PromoCodeDto()
                {
                    Discount = existingInProgressPromoCodeRedemption.ValueRedeemed,
                    FinalCost = existingInProgressPromoCodeRedemption.CourseVariant.Price - existingInProgressPromoCodeRedemption.ValueRedeemed,
                    IsValid = true,
                    ValidationMessage = $"The promo code '{existingInProgressPromoCodeRedemption.PromoCode.PromoName}' has been applied successfully! See below for the updated price.",
                    PromoCodeRedemptionGuid = existingInProgressPromoCodeRedemption.PromoCodeRedemptionGuid,
                    PromoDescription = existingInProgressPromoCodeRedemption.PromoCode.PromoDescription,
                    PromoName = existingInProgressPromoCodeRedemption.PromoCode.PromoName
                };
            }

            return validPromoCodeDto;


        }



    }
}
