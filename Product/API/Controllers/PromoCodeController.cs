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
        [Route("api/[controller]/{code}/{courseGuid}/{subscriberGuid}")]
        public IActionResult PromoCodeValidation(string code, string courseGuid, string subscriberGuid, bool isRedemptionStarted = false)
        {
            try
            {
                /*  todo: refactor this code. move business rules to IValidatableObject in Dto? to do this, i think we would need to
                *   use a custom value resolver in automapper to transform 5 model objects (PromoCode, Course, CoursePromoCode, 
                *   VendorPromoCode, SubscriberPromoCode) into a PromoCodeDto. once all of the necessary properties exist within that 
                *   Dto, then we could move all of the validation to:
                *       IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
                */

                #region business logic to refactor

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

                if (promoCode.NumberOfRedemptions >= promoCode.MaxAllowedNumberOfRedemptions)
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "This promo code has exceeded its allowed number of redemptions." });

                Guid parsedCourseGuid;
                Guid.TryParse(courseGuid, out parsedCourseGuid);
                Course course = _db.Course
                    .Where(c => c.CourseGuid == parsedCourseGuid)
                    .FirstOrDefault();

                if (course == null)
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "The course specified is invalid." });

                List<CoursePromoCode> courseRestrictionsForThisPromoCode = _db.CoursePromoCode
                    .Where(cpc => cpc.PromoCodeId == promoCode.PromoCodeId)
                    .ToList();

                if (courseRestrictionsForThisPromoCode.Any() && !courseRestrictionsForThisPromoCode.Any(r => r.CourseId == course.CourseId))
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "Promo code is not valid for this course." });

                List<VendorPromoCode> vendorRestrictionsForThisPromoCode = _db.VendorPromoCode
                    .Where(vpc => vpc.PromoCodeId == promoCode.PromoCodeId)
                    .ToList();

                if (vendorRestrictionsForThisPromoCode.Any() && !vendorRestrictionsForThisPromoCode.Any(vpc => vpc.VendorId == course.VendorId))
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "Promo code is not valid for this vendor." });

                Guid parsedSubscriberGuid;
                Guid.TryParse(subscriberGuid, out parsedSubscriberGuid);
                Subscriber subscriber = _db.Subscriber
                    .Where(s => s.SubscriberGuid == parsedSubscriberGuid)
                    .FirstOrDefault();

                if (subscriber == null)
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "The subscriber specified is invalid." });

                List<SubscriberPromoCode> subscriberRestrictionsForThisPromoCode = _db.SubscriberPromoCode
                    .Where(spc => spc.PromoCodeId == promoCode.PromoCodeId)
                    .ToList();

                if (subscriberRestrictionsForThisPromoCode.Any() && !subscriberRestrictionsForThisPromoCode.Any(spc => spc.SubscriberId == subscriber.SubscriberId))
                    return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "Promo code is not valid for this subscriber." });
                #endregion

                PromoCodeDto validPromoCode = _mapper.Map<PromoCodeDto>(promoCode);
                validPromoCode.IsValid = true;
                switch (promoCode.PromoType.Name)
                {
                    case "Dollar Amount":
                        validPromoCode.Discount = !course.Price.HasValue ? 0 : Math.Max(0, promoCode.PromoValueFactor);
                        break;
                    case "Percent Off":
                        validPromoCode.Discount = !course.Price.HasValue ? 0 : Math.Max(0, Math.Round(course.Price.Value * promoCode.PromoValueFactor, 2, MidpointRounding.ToEven));
                        break;
                    default:
                        throw new ApplicationException("Unrecognized promo type!");
                }

                validPromoCode.FinalCost = !course.Price.HasValue ? 0 : course.Price.Value - validPromoCode.Discount;

                return Ok(validPromoCode);
            }
            catch (Exception e)
            {
                // todo: logging?
                return Ok(new PromoCodeDto() { IsValid = false, ValidationMessage = "An unexpected error occurred." });
            }
        }
    }
}