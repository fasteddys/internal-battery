using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
                _db.SaveChanges();
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
            Decimal amount = 100; //TODO Get amount from Bill's endpoint

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
                Amount = amount,
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


    }
}
