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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using AutoMapper.QueryableExtensions;
using UpDiddyApi.Workflow;
using Hangfire;
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

   
        [HttpPost]
        [Route("api/[controller]")]
        public IActionResult Post([FromBody] EnrollmentDto EnrollmentDto)
        {

            try
            {
                Enrollment Enrollment = _mapper.Map<Enrollment>(EnrollmentDto);
                _db.Enrollment.Add(Enrollment);
                if (EnrollmentDto.PromoCodeRedemptionGuid.HasValue)
                {
                    //var promoCodeRedemption = _db.PromoCodeRedemption.Where(pcr => pcr.PromoCodeRedemptionGuid == EnrollmentDto.PromoCodeRedemptionGuid).FirstOrDefault();
                    //promoCodeRedemption.ModifyDate = DateTime.UtcNow;
                    //promoCodeRedemption.ModifyGuid = Guid.NewGuid();
                    //promoCodeRedemption.RedemptionStatusId = 2; // completed

                    // todo: decrement the field in PromoCodes that keeps track of how many codes have been used
                    //_db.Attach<PromoCodeRedemption>(promoCodeRedemption);
                }
                _db.SaveChanges();
                BackgroundJob.Enqueue<WozEnrollmentFlow>(x => x.EnrollStudentWorkItem(EnrollmentDto.EnrollmentGuid.ToString()));
                return Ok(Enrollment.EnrollmentGuid);
            }
            catch ( Exception ex )
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex);
            }
           
        }

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

        private Decimal? CalculatePromoCodeDiscountedPrice(Decimal? CoursePrice, Decimal PromoValueFactor)
        {
            return (CoursePrice - CalculatePriceOffOfCourse(CoursePrice, PromoValueFactor));
        }

        private Decimal CalculatePriceOffOfCourse(Decimal? CoursePrice, Decimal PromoValueFactor)
        {
            return (Math.Floor((Decimal)CoursePrice * PromoValueFactor * 100)) / 100;
        }

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
