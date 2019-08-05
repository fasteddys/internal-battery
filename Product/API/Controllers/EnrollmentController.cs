using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using UpDiddyApi.Workflow;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UpDiddyApi.ApplicationCore;
using Microsoft.Extensions.Caching.Distributed;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyLib.MessageQueue;
using UpDiddyApi.ApplicationCore.Interfaces;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
 
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        protected internal ILogger _syslog = null;
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHangfireService _hangfireService;


        public EnrollmentController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<EnrollmentController> sysLog, IHttpClientFactory httpClientFactory, IDistributedCache distributedCache, IHangfireService hangfireService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _distributedCache = distributedCache;
            _httpClientFactory = httpClientFactory;
            _hangfireService = hangfireService;
        }

       
        [Authorize(Policy = "IsCareerCircleAdmin")]  
        [HttpDelete]
        [Route("api/[controller]/{EnrollmentGuid}")]
        public IActionResult DeleteEnrollment( Guid EnrollmentGuid )
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                return BadRequest();

            Enrollment enrollment = EnrollmentFactory.GetEnrollmentByGuid(_db, EnrollmentGuid);
            if ( enrollment == null )
                return BadRequest(new BasicResponseDto() { StatusCode = 404, Description = $"Enrollment {EnrollmentGuid} not found" });

            bool DeletedOk = true;
            string ReturnInfo = string.Empty;
            // Check to see if its a woz enrollment 
            WozCourseEnrollment wozCourseEnrollment = WozCourseEnrollmentFactory.GetWozCourseEnrollmentByEnrollmentGuid(_db, enrollment.EnrollmentGuid.Value);
            if (wozCourseEnrollment != null)
            {                
                WozInterface wozInterface = new WozInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
                MessageTransactionResponse rVal = wozInterface.UnEnrollStudent(enrollment, wozCourseEnrollment.SectionId, wozCourseEnrollment.WozEnrollmentId);
                if (rVal.State == TransactionState.InProgress)
                {
                    wozCourseEnrollment.IsDeleted = 1;
                    wozCourseEnrollment.ModifyDate = DateTime.UtcNow;
                    wozCourseEnrollment.ModifyGuid = subscriberGuid;
                    wozCourseEnrollment.EnrollmentStatus = (int) WozEnrollmentStatus.Withdrawn;
                    ReturnInfo = rVal.InformationalMessage;
                }
                else
                {
                    DeletedOk = false;
                    ReturnInfo = "Error deleting enrollment with Woz" + rVal.InformationalMessage;
                }
            }
            else
                ReturnInfo = "Not a Woz enrollment";
            
            if ( DeletedOk)
            {
                // Mark the enrollment as deleted 
                enrollment.IsDeleted = 1;
                enrollment.ModifyDate = DateTime.UtcNow;
                enrollment.ModifyGuid = subscriberGuid;
                _db.SaveChanges();
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = ReturnInfo });
            }
            else
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ReturnInfo });
            
        }
        

        [Authorize]
        [HttpPost]
        [Route("api/[controller]")]
        public IActionResult Post([FromBody] EnrollmentFlowDto EnrollmentFlowDto)
        {
            EnrollmentDto EnrollmentDto = EnrollmentFlowDto.EnrollmentDto;
            BraintreePaymentDto BraintreePaymentDto = EnrollmentFlowDto.BraintreePaymentDto;

            // check subscriber that is logged in vs passed in via body
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);


            // check EnrolmmentDto but if exception occurs then this is a bad request
            try
            {
                if (!subscriberGuid.Equals(EnrollmentFlowDto.SubscriberDto.SubscriberGuid))
                    return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = 400, message = "Missing required data in EnrollmentDto." });
            }

            try
            {
                // grab the subscriber information we need for the enrollment log, then set the property to null on EnrollmentDto so that ef doesn't try to create the subscriber
                EnrollmentDto.Subscriber = null;
                EnrollmentDto.CourseId = _db.Course.Where(c => c.CourseGuid == EnrollmentDto.CourseGuid).Select(c => c.CourseId).FirstOrDefault();
                Enrollment Enrollment = _mapper.Map<Enrollment>(EnrollmentDto);

                if (Enrollment.CampaignCourseVariant != null)
                {
                    Enrollment.CampaignId = EnrollmentDto.CampaignCourseVariant.Campaign.CampaignId;
                    Enrollment.CourseVariantId = EnrollmentDto.CampaignCourseVariant.CourseVariant.CourseVariantId;
                    Enrollment.CampaignCourseVariant = null;
                }
                _db.Enrollment.Add(Enrollment);

                /* todo: need to revisit the enrollment log for a number of reasons: 
                 *      mismatch for "required" between properties of this and related entities (e.g. SubscriberGuid)
                 *      what does EnrollmentTime mean, how does this differ from SectionStartTimestamp, DateEnrolled, or even just CreateDate
                 *      does it make sense for PromoApplied to be required?
                 *      redundancy between CourseVariantGuid and CourseCost - this may cause problems (e.g. CourseCost)
                 *      currently payment status is fixed because we are processing the BrainTree payment synchronously; this will not be the case in the future
                 *      the payment month and year is 30 days after the enrollment date for Woz, will be different for other vendors (forcing us to address that when we onboard the next vendor)
                 */
                DateTime currentDate = DateTime.UtcNow;
                var course = _db.Course.Where(c => c.CourseId == EnrollmentDto.CourseId).FirstOrDefault();
                var vendor = _db.Vendor.Where(v => v.VendorId == course.VendorId).FirstOrDefault(); // why is vendor id nullable on course?

                var originalCoursePrice = _db.CourseVariant
                    .Where(cv => cv.CourseVariantGuid == EnrollmentDto.CourseVariantGuid)
                    .Select(cv => cv.Price)
                    .FirstOrDefault();

                var promoCodeRedemption = _db.PromoCodeRedemption
                    .Include(pcr => pcr.RedemptionStatus)
                    .Where(pcr => pcr.PromoCodeRedemptionGuid == EnrollmentDto.PromoCodeRedemptionGuid && pcr.RedemptionStatus.Name == "Completed").FirstOrDefault();
                int paymentMonth = 0;
                int paymentYear = 0;

                if (vendor == null || vendor.Name == "WozU")
                {
                    // calculate vendor invoice payment month and year
                    paymentMonth = currentDate.AddDays(30).Month;
                    paymentYear = currentDate.AddDays(30).Year;
                }
                else
                {
                    // force us to clean this up once we have more vendors
                    throw new ApplicationException("Unrecognized vendor; cannot calculate vendor invoice payment month and year");
                }

                _db.EnrollmentLog.Add(new EnrollmentLog()
                {
                    CourseCost = originalCoursePrice,
                    CourseGuid = course.CourseGuid.HasValue ? course.CourseGuid.Value : Guid.Empty,
                    CourseVariantGuid = EnrollmentDto.CourseVariantGuid,
                    CreateDate = currentDate,
                    CreateGuid = Guid.Empty,
                    EnrollmentGuid = EnrollmentDto.EnrollmentGuid.Value,
                    EnrollmentTime = currentDate,
                    EnrollmentLogGuid = Guid.NewGuid(),
                    EnrollmentVendorInvoicePaymentMonth = paymentMonth,
                    EnrollmentVendorInvoicePaymentYear = paymentYear,
                    PromoApplied = (promoCodeRedemption != null) ? promoCodeRedemption.ValueRedeemed : 0,
                    SubscriberGuid = subscriberGuid,
                    EnrollmentVendorPaymentStatusId = 2
                });

                _db.SaveChanges();

                /**
                 *  This line used to enqueue the enrollment flow. Now, it's enqueuing the braintree flow,
                 *  which will then enqueue the enrollment flow if the payment is successful.
                 */
                _hangfireService.Enqueue<BraintreePaymentFlow>(x => x.PaymentWorkItem(EnrollmentFlowDto));

                return Ok(Enrollment.EnrollmentGuid);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex);
            }

        }

        // todo: maybe consider making this as part enrollment resource or a query that WebApp can make
        [Authorize]
        [HttpGet]
        [Route("api/[controller]/{EnrollmentGuid}/student-login-url")]
        public IActionResult StudentLoginUrl(Guid EnrollmentGuid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // todo: course login dto is one property, thus maybe we can add that property as part of the enrollment in general
            var CourseLogin = new CourseFactory(_db, _configuration, _syslog, _distributedCache, _hangfireService)
                .GetCourseLogin(subscriberGuid, EnrollmentGuid);
            return Ok(CourseLogin);
        }
    }
}
