using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Helpers;
using UpDiddyLib.Helpers.Braintree;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Braintree;
using UpDiddyLib.Helpers;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using AutoMapper.QueryableExtensions;
using UpDiddyApi.Workflow;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
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
        private readonly string _queueConnection = string.Empty;
        private readonly CCQueue _queue = null;
        private IBraintreeConfiguration braintreeConfiguration;
        public EnrollmentController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _queue = new CCQueue("ccmessagequeue", _queueConnection);
            braintreeConfiguration = new BraintreeConfiguration(_configuration);
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]")]
        public IActionResult Post([FromBody] EnrollmentDto EnrollmentDto)
        {

            try
            {
                // grab the subscriber information we need for the enrollment log, then set the property to null on EnrollmentDto so that ef doesn't try to create the subscriber
                Guid subscriberGuid = EnrollmentDto.Subscriber.SubscriberGuid.Value;
                EnrollmentDto.Subscriber = null;

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
                var vendor = _db.Vendor.Where(v => v.VendorId == course.VendorId.Value).FirstOrDefault(); // why is vendor id nullable on course?
                var courseVariant = _db.CourseVariant.Where(cv => cv.CourseVariantId == EnrollmentDto.CourseVariantId).FirstOrDefault();
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
                    CourseCost = (courseVariant != null && courseVariant.Price.HasValue) ? courseVariant.Price.Value : (course.Price.HasValue) ? course.Price.Value : 0,
                    CourseGuid = course.CourseGuid.HasValue ? course.CourseGuid.Value : Guid.Empty,
                    CourseVariantGuid = courseVariant.CourseVariantGuid,
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

                BackgroundJob.Enqueue<WozEnrollmentFlow>(x => x.EnrollStudentWorkItem(EnrollmentDto.EnrollmentGuid.ToString()));
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
            var gateway = braintreeConfiguration.GetGateway();
            var nonce = BraintreePaymentDto.Nonce;
            AddressRequest addressRequest;


            addressRequest = new AddressRequest
            {
                // Assuming form fields are filled in at this point until above TODO is handled
                FirstName = BraintreePaymentDto.FirstName,
                LastName = BraintreePaymentDto.LastName,
                StreetAddress = BraintreePaymentDto.Address,
                Region = BraintreePaymentDto.Region,
                Locality = BraintreePaymentDto.Locality,
                PostalCode = BraintreePaymentDto.ZipCode,
                CountryCodeAlpha2 = BraintreePaymentDto.CountryCode

            };

            TransactionRequest request = new TransactionRequest
            {
                Amount = BraintreePaymentDto.PaymentAmount,
                MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("BraintreeMerchantAccountID"),
                PaymentMethodNonce = nonce,
                Customer = new CustomerRequest
                {
                    FirstName = BraintreePaymentDto.FirstName,
                    LastName = BraintreePaymentDto.LastName,
                    Phone = BraintreePaymentDto.PhoneNumber,
                    Email = BraintreePaymentDto.Email
                },
                BillingAddress = addressRequest,
                ShippingAddress = new AddressRequest
                {
                    FirstName = BraintreePaymentDto.FirstName,
                    LastName = BraintreePaymentDto.LastName,
                    StreetAddress = BraintreePaymentDto.Address
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);
            return new BraintreeResponseDto
            {
                WasSuccessful = result.IsSuccess()
            };
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

        // TODO Use SubcriberGuid 
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
