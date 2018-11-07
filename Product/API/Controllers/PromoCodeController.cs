using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class PromoCodeController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public PromoCodeController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
        }


        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {

            IList<PromoCodeDto> rval = null;
            rval = _db.PromoCode
                .Where(t => t.IsDeleted == 0)
                .ProjectTo<PromoCodeDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rval);

        }

        [Authorize]
        [HttpGet]
        [Route("api/[controller]/PromoCodeRedemptionValidation/{promoCodeRedemptionGuid}/{courseVariantGuid}/{subscriberGuid}")]
        public IActionResult PromoCodeRedemptionValidation(string promoCodeRedemptionGuid, string courseVariantGuid, string subscriberGuid)
        {
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
                         && cv.CourseVariantGuid == Guid.Parse(courseVariantGuid)
                         && s.SubscriberGuid == Guid.Parse(subscriberGuid)
                         && s.IsDeleted == 0
                         && pcr.PromoCodeRedemptionGuid == Guid.Parse(promoCodeRedemptionGuid)
                         && pc.NumberOfRedemptions < pc.MaxAllowedNumberOfRedemptions
                         select new PromoCodeDto()
                         {
                             Discount = pcr.ValueRedeemed,
                             IsValid = true,
                             PromoCodeRedemptionGuid = pcr.PromoCodeRedemptionGuid
                         }).FirstOrDefault();
            
            if (query != null)
            {
                // mark the promo code as completed
                var completedPromoCodeRedemption = _db.PromoCodeRedemption.Where(pcr => pcr.PromoCodeRedemptionGuid == query.PromoCodeRedemptionGuid).FirstOrDefault();
                completedPromoCodeRedemption.ModifyDate = DateTime.UtcNow;
                completedPromoCodeRedemption.ModifyGuid = Guid.NewGuid();
                completedPromoCodeRedemption.RedemptionStatusId = 2; // completed
                _db.Attach<PromoCodeRedemption>(completedPromoCodeRedemption);
                
                // increment the number of redemptions for the code
                var promoCodeId = (from pc in _db.PromoCode
                             join pcr in _db.PromoCodeRedemption on pc.PromoCodeId equals pcr.PromoCodeId
                             where pcr.PromoCodeRedemptionGuid == Guid.Parse(promoCodeRedemptionGuid)
                             select pc.PromoCodeId).FirstOrDefault();

                var promoCodeToUpdate = _db.PromoCode.Where(pc => pc.PromoCodeId == promoCodeId).FirstOrDefault();
                promoCodeToUpdate.ModifyDate = DateTime.UtcNow;
                promoCodeToUpdate.ModifyGuid = Guid.NewGuid();
                promoCodeToUpdate.NumberOfRedemptions += 1;
                _db.Attach<PromoCode>(promoCodeToUpdate);

                _db.SaveChanges();
            }

            return Ok(query);
        }

        [Authorize]
        [HttpGet]
        [Route("api/[controller]/{code}/{courseVariantGuid}/{subscriberGuid}")]
        public IActionResult PromoCodeValidation(string code, string courseVariantGuid, string subscriberGuid)
        {
            try
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
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "This promo code does not exist." });

                if (promoCode.IsDeleted == 1)
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "This promo code is no longer valid." });

                DateTime currentDateTime = DateTime.Now;
                if (promoCode.PromoStartDate > currentDateTime.AddHours(-4)) // todo: improve ghetto grace period date logic
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "This promo code is not yet active." });

                if (promoCode.PromoEndDate < currentDateTime.AddHours(4)) // todo: improve ghetto grace period date logic
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "This promo code has expired." });

                var inProgressRedemptionsForThisCode = _db.PromoCodeRedemption
                    .Include(pcr => pcr.RedemptionStatus)
                    .Where(pcr => pcr.PromoCodeId == promoCode.PromoCodeId && pcr.IsDeleted == 0 && pcr.RedemptionStatus.Name == "In Process")
                    .Count();
                if (promoCode.NumberOfRedemptions >= promoCode.MaxAllowedNumberOfRedemptions || promoCode.NumberOfRedemptions + inProgressRedemptionsForThisCode > promoCode.MaxAllowedNumberOfRedemptions)
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "This promo code has exceeded its allowed number of redemptions." });

                Guid parsedVariantCourseGuid;
                Guid.TryParse(courseVariantGuid, out parsedVariantCourseGuid);
                CourseVariant courseVariant = _db.CourseVariant
                    .Where(cv => cv.CourseVariantGuid == parsedVariantCourseGuid)
                    .FirstOrDefault();

                if (courseVariant == null)
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "The course specified is invalid." });

                List<CourseVariantPromoCode> courseRestrictionsForThisPromoCode = _db.CourseVariantPromoCode
                    .Where(cpc => cpc.PromoCodeId == promoCode.PromoCodeId)
                    .ToList();

                if (courseRestrictionsForThisPromoCode.Any() && !courseRestrictionsForThisPromoCode.Any(r => r.CourseVariantId == courseVariant.CourseVariantId))
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "Promo code is not valid for this course." });

                List<VendorPromoCode> vendorRestrictionsForThisPromoCode = _db.VendorPromoCode
                    .Where(vpc => vpc.PromoCodeId == promoCode.PromoCodeId)
                    .ToList();

                // todo: not sure this works? if it does, need to clean it up, guard against null refs, etc
                var course = _db.Course
                    .Include(c => c.CourseVariants)
                    .Where(c => c.CourseVariants
                        .Where(cv => cv.CourseVariantId == courseVariant.CourseVariantId)
                        .Any()
                    ).FirstOrDefault();
                
                if (vendorRestrictionsForThisPromoCode.Any() && !vendorRestrictionsForThisPromoCode.Any(vpc => vpc.VendorId == course.VendorId))
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "Promo code is not valid for this vendor." });

                Guid parsedSubscriberGuid;
                Guid.TryParse(subscriberGuid, out parsedSubscriberGuid);
                Subscriber subscriber = _db.Subscriber
                    .Where(s => s.SubscriberGuid == parsedSubscriberGuid && s.IsDeleted == 0)
                    .FirstOrDefault();

                if (subscriber == null)
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "The subscriber specified is invalid." });

                List<SubscriberPromoCode> subscriberRestrictionsForThisPromoCode = _db.SubscriberPromoCode
                    .Where(spc => spc.PromoCodeId == promoCode.PromoCodeId)
                    .ToList();

                if (subscriberRestrictionsForThisPromoCode.Any() && !subscriberRestrictionsForThisPromoCode.Any(spc => spc.SubscriberId == subscriber.SubscriberId))
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "Promo code is not valid for this subscriber." });

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
                            validPromoCodeDto.Discount =  Math.Max(0, promoCode.PromoValueFactor);
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
                        CreateGuid = Guid.NewGuid(),
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

                return Ok(validPromoCodeDto);
            }
            catch (Exception e)
            {
                // todo: logging?
                return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "An unexpected error occurred." });
            }
        }
    }
}