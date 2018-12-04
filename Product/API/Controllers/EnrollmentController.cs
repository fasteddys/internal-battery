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
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    //TODO [Authorize]
    [ApiController]
    public class EnrollmentController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;       
        protected internal ILogger _syslog = null;

        public EnrollmentController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<EnrollmentController> sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;      
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]")]
        public IActionResult Post([FromBody] EnrollmentFlowDto EnrollmentFlowDto)
        {
            EnrollmentDto EnrollmentDto = EnrollmentFlowDto.EnrollmentDto;
            BraintreePaymentDto BraintreePaymentDto = EnrollmentFlowDto.BraintreePaymentDto;
            try
            {
                // grab the subscriber information we need for the enrollment log, then set the property to null on EnrollmentDto so that ef doesn't try to create the subscriber
                Guid subscriberGuid = EnrollmentDto.Subscriber.SubscriberGuid.Value;
                EnrollmentDto.Subscriber = null;
                EnrollmentDto.CourseId = _db.Course.Where(c => c.CourseGuid == EnrollmentDto.CourseGuid).Select(c => c.CourseId).FirstOrDefault();
                Enrollment Enrollment = _mapper.Map<Enrollment>(EnrollmentDto);
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
                BackgroundJob.Enqueue<BraintreePaymentFlow>(x => x.PaymentWorkItem(EnrollmentFlowDto));

                return Ok(Enrollment.EnrollmentGuid);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex);
            }

        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/EnrollmentLog")]
        public IActionResult Post([FromBody] EnrollmentLogDto EnrollmentLogDto)
        {
            try
            {
                EnrollmentLog EnrollmentLog = _mapper.Map<EnrollmentLog>(EnrollmentLogDto);
                _db.EnrollmentLog.Add(EnrollmentLog);
                _db.SaveChanges();
                return Ok(EnrollmentLog.EnrollmentGuid);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/ProcessBraintreePayment")]
        public BraintreeResponseDto ProcessBraintreePayment([FromBody] BraintreePaymentDto BraintreePaymentDto)
        {
            return new BraintreeResponseDto();
            /*
            BackgroundJob.Enqueue<BraintreePaymentFlow>(x => x.PaymentWorkItem(BraintreePaymentDto));



            return new BraintreeResponseDto
            {
                WasSuccessful = result.IsSuccess()
            };
            */
        }
        [Authorize]
        [HttpGet]
        [Route("api/[controller]/CurrentEnrollments/{SubscriberId}")]
        public IActionResult GetCurrentEnrollmentsForSubscriber(int SubscriberId)
        {
            IList<EnrollmentDto> rval = null;
            rval = _db.Enrollment
                .Where(t => t.IsDeleted == 0 && t.SubscriberId == SubscriberId)
                .ProjectTo<EnrollmentDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rval);
        }

     
        [Authorize]
        [HttpGet]
        [Route("api/[controller]/StudentLogin/{SubscriberId}")]
        public IActionResult StudentLogin(int SubscriberId)
        {
            VendorStudentLoginDto rval = null;
            rval = _db.VendorStudentLogin
                .Where(t => t.IsDeleted == 0 && t.SubscriberId == SubscriberId)
                .ProjectTo<VendorStudentLoginDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
            return Ok(rval);
        }
    }
}
