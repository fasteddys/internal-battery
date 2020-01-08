using AutoMapper;
using Braintree;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyLib.Helpers.Braintree;
using UpDiddyLib.MessageQueue;
using EnrollmentStatus = UpDiddyLib.Dto.EnrollmentStatus;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Workflow
{
    public class BraintreePaymentFlow
    {

        private UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private IConfiguration _configuration;
        private ISysEmail _sysEmail = null;
        private ILogger _sysLog = null;
        private IBraintreeConfiguration _braintreeConfiguration = null;
        private readonly IHangfireService _hangfireService;


        public BraintreePaymentFlow(UpDiddyDbContext dbcontext, IMapper mapper, IConfiguration configuration, ISysEmail sysEmail, IServiceProvider serviceProvider, ILogger<BraintreePaymentFlow> logger, IHangfireService hangfireService)
        {
            _db = dbcontext;
            _mapper = mapper;
            _configuration = configuration;
            _sysEmail = sysEmail;
            _sysLog = logger;
            _braintreeConfiguration = new BraintreeConfiguration(_configuration);
            _hangfireService = hangfireService;
        }

        public MessageTransactionResponse PaymentWorkItem(EnrollmentFlowDto EnrollmentFlowDto)
        {

            _sysLog.LogInformation("BraintreePaymentFlow:PaymentWorkItem Starting");
            // Extract the two DTOs from the DTO that's passed in.
            BraintreePaymentDto BraintreePaymentDto = null;
            EnrollmentDto EnrollmentDto = null;
            SubscriberDto SubscriberDto = null;
            WorkflowHelper Helper = new WorkflowHelper(_db, _configuration, _sysLog);

            try
            {
                BraintreePaymentDto = EnrollmentFlowDto.BraintreePaymentDto;
                EnrollmentDto = EnrollmentFlowDto.EnrollmentDto;
                SubscriberDto = EnrollmentFlowDto.SubscriberDto;
            }
            catch (Exception e)
            {
                string ErrorMessage = "BraintreePaymentFlow:PaymentWorkItem -> No Braintree, subscriber or enrollment dto present; enrollment has been cancelled.";
                _sysLog.Log(LogLevel.Critical, default(EventId), e,
                    "BraintreePaymentFlow:PaymentWorkItem -> No Braintree, subscriber or enrollment dto present; enrollment has been cancelled. {@EnrollmentFlowDto}",
                    new[] { EnrollmentFlowDto }
                    );
                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentError);
                return CreateResponse(CreateResponseJson(ErrorMessage), ErrorMessage, string.Empty, TransactionState.FatalError);
            }

            _sysLog.LogInformation("BraintreePaymentFlow:PaymentWorkItem Getting Payment Gateway");

            IBraintreeGateway gateway = _braintreeConfiguration.GetGateway();
            string nonce = BraintreePaymentDto.Nonce;
            TransactionRequest TransactionRequest = null;

            // Make sure Nonce is present when there is a non-zero payment amount
            if (string.IsNullOrEmpty(nonce) && BraintreePaymentDto.PaymentAmount > 0)
            {
                string NullOrEmptyMessage = "BraintreePaymentFlow:PaymentWorkItem -> Braintree nonce is null or empty; enrollment has been cancelled.";
                _sysLog.Log(LogLevel.Critical,
                    "BraintreePaymentFlow:PaymentWorkItem -> Braintree nonce is null or empty; enrollment has been cancelled. {@EnrollmentFlowDto}",
                    new[] { EnrollmentFlowDto }
                    );
                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentError);
                return CreateResponse(CreateResponseJson(NullOrEmptyMessage), NullOrEmptyMessage, string.Empty, TransactionState.FatalError);
            }

            // Create the transaction request object
            try
            {
                _sysLog.LogInformation("BraintreePaymentFlow:PaymentWorkItem Assembling transaction ");
                TransactionRequest = AssembleTransactionRequest(BraintreePaymentDto);
            }
            catch (Exception e)
            {
                string ExceptionString = "BraintreePaymentFlow:PaymentWorkItem -> An error occurred when attempting to create the TransactionRequest.";
                _sysLog.Log(LogLevel.Critical, default(EventId), e,
                    "BraintreePaymentFlow:PaymentWorkItem -> An error occurred when attempting to create the TransactionRequest. {@EnrollmentFlowDto}",
                    new[] { EnrollmentFlowDto }
                    );

                Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentError);
                return CreateResponse(CreateResponseJson(ExceptionString), ExceptionString, string.Empty, TransactionState.FatalError);
            }

            try
            {
                // the following variable exists to differentiate between a successful payment and a successful transaction. 
                // this is necessary because courses can be free with a promo code, in which case no payment is made. 
                // in this case, the transaction is still successful even though no payment was made.
                bool isTransactionSuccessful = false;

                Result<Transaction> paymentResult = null;

                if (BraintreePaymentDto.PaymentAmount > 0)
                {
                    paymentResult = gateway.Transaction.Sale(TransactionRequest);
                    Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentComplete);
                    _sysLog.Log(LogLevel.Information, "BraintreePaymentFlow:PaymentWorkItem -> Braintree payment in progress.");
                    if (paymentResult.IsSuccess())
                        isTransactionSuccessful = true;
                }
                else
                {
                    // payment amount was zero, therefore we do not attempt to process a payment. 
                    // transaction is considered to be successful.
                    isTransactionSuccessful = true;
                }

                _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem isTransactionSuccessful = {isTransactionSuccessful}");

                if (isTransactionSuccessful)
                {
                    string SuccessfulMessage = "BraintreePaymentFlow:PaymentWorkItem->Braintree payment was successful.";
                    _sysLog.Log(LogLevel.Information, SuccessfulMessage);
                    Course course = _db.Course.Where(t => t.IsDeleted == 0 && t.CourseId == EnrollmentDto.CourseId).FirstOrDefault();
                    EnrollmentLog enrollmentLog = _db.EnrollmentLog.Where(t => t.IsDeleted == 0 && t.EnrollmentGuid == EnrollmentDto.EnrollmentGuid).FirstOrDefault();
                    if ( enrollmentLog != null )
                        _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Enrollment Log for enrollment {EnrollmentDto.EnrollmentGuid} found!");
                    else
                        _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Enrollment Log for enrollment {EnrollmentDto.EnrollmentGuid} NOT found!");

                    CourseVariant courseVariant = _db.CourseVariant.Include(cv => cv.CourseVariantType).Where(cv => cv.IsDeleted == 0 && cv.CourseVariantGuid.Value == enrollmentLog.CourseVariantGuid.Value).FirstOrDefault();
                    if (courseVariant != null)
                        _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Course Variant for enrollment log  {enrollmentLog.CourseVariantGuid.Value} found!");
                    else
                        _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Course Variant for enrollment log  {enrollmentLog.CourseVariantGuid.Value} NOT found!");

                    Enrollment enrollment = _db.Enrollment.Where(e => e.IsDeleted == 0 && e.EnrollmentGuid.Value == enrollmentLog.EnrollmentGuid)
                        .Include(e => e.CampaignCourseVariant)
                        .ThenInclude(ccv => ccv.RebateType)
                        .FirstOrDefault();

                    if (enrollment != null)
                        _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Enrollment  {enrollmentLog.EnrollmentGuid } found!");
                    else
                        _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Enrollment  {enrollmentLog.EnrollmentGuid } Not found!");



                    string formattedStartDate = enrollment.SectionStartTimestamp.HasValue ? Utils.FromUnixTimeInMilliseconds(enrollment.SectionStartTimestamp.Value).ToShortDateString() : string.Empty;
                    string templateId = null;
                    switch (courseVariant.CourseVariantType.Name)
                    {
                        case "Self-Paced":
                            templateId= _configuration["SysEmail:Transactional:TemplateIds:PurchaseReceipt-SelfPaced"];
                            break;
                        case "Instructor-Led":
                            templateId = _configuration["SysEmail:Transactional:TemplateIds:PurchaseReceipt-InstructorLed"];
                            break;
                        default:
                            throw new ApplicationException("Unrecognized course variant type.");
                    }

                    _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Email template for variant type {courseVariant.CourseVariantType.Name} has Id =   {templateId}");

                    string profileUrl = _configuration["Environment:BaseUrl"] + "Home/Profile"; // todo: once we aren't using register links from profile page, generate link to Woz course
                    string courseType = courseVariant.CourseVariantType.Name;

                    // check to see if the enrollment was part of a campaign
                    string rebateToc = string.Empty;
                    if(enrollment.CampaignCourseVariant != null)
                        rebateToc = enrollment.CampaignCourseVariant.RebateType.Terms;

                    _sysLog.LogInformation($"BraintreePaymentFlow:PaymentWorkItem Sending receipt email for course {course.Name}  CourseCost = {enrollmentLog.CourseCost} PromoApplied = {enrollmentLog.PromoApplied}");

                    _sysEmail.SendPurchaseReceiptEmail(
                        templateId, 
                        profileUrl, 
                        SubscriberDto.Email, 
                        $"Purchase Receipt For {courseType} CareerCircle Course", 
                        course.Name, 
                        enrollmentLog.CourseCost, 
                        enrollmentLog.PromoApplied, 
                        formattedStartDate, 
                        (Guid)EnrollmentDto.EnrollmentGuid, 
                        rebateToc);
                    SetSelfPacedOrInstructorLedStatus(Helper, EnrollmentDto);
                    _hangfireService.Enqueue<WozEnrollmentFlow>(x => x.EnrollStudentWorkItem(EnrollmentDto.EnrollmentGuid.ToString(), EnrollmentDto.SubscriberId));
                    return CreateResponse(CreateResponseJson(SuccessfulMessage), SuccessfulMessage, string.Empty, TransactionState.Complete);
                }
                else
                {
                    string UnsuccessfulMessage = "BraintreePaymentFlow:PaymentWorkItem->Braintree payment was unsuccessful.";
                    _sysLog.Log(LogLevel.Critical,
                        "BraintreePaymentFlow:PaymentWorkItem->Braintree payment was unsuccessful. {@EnrollmentFlowDto}",
                        new[] { EnrollmentFlowDto }
                        );

                    Helper.UpdateEnrollmentStatus(EnrollmentDto.EnrollmentGuid.ToString(), UpDiddyLib.Dto.EnrollmentStatus.PaymentFatalError);
                    return CreateResponse(CreateResponseJson(UnsuccessfulMessage), UnsuccessfulMessage, string.Empty, TransactionState.Error);
                }

            }
            catch (Exception ex)
            {
                string ExceptionString = "BraintreePaymentFlow:PaymentWorkItem -> An error occurred";
                _sysLog.Log(LogLevel.Critical, default(EventId), ex,
                    "BraintreePaymentFlow:PaymentWorkItem -> An error occurred. {@EnrollmentFlowDto}",
                    new[] { EnrollmentFlowDto }
                    );
                return CreateResponse(CreateResponseJson(ExceptionString), ExceptionString, string.Empty, TransactionState.FatalError);
            }
        }

        private void SetSelfPacedOrInstructorLedStatus(WorkflowHelper Helper, EnrollmentDto Enrollment)
        {
            Helper.UpdateEnrollmentStatus(Enrollment.EnrollmentGuid.ToString(), (EnrollmentStatus)Enrollment.EnrollmentStatusId);
        }

        private TransactionRequest AssembleTransactionRequest(BraintreePaymentDto BraintreePaymentObject)
        {
            if (BraintreePaymentObject.StateGuid.HasValue)
            {
                State state = _db.State.Where(s => s.StateGuid == BraintreePaymentObject.StateGuid.Value).FirstOrDefault();
                if (state != null)
                {
                    BraintreePaymentObject.Region = state.Code;
                }
            }
            if (BraintreePaymentObject.CountryGuid.HasValue)
            {
                Country country = _db.Country.Where(c => c.CountryGuid == BraintreePaymentObject.CountryGuid.Value).FirstOrDefault();
                if (country != null)
                {
                    BraintreePaymentObject.CountryCode = country.Code2;
                }
            }

            TransactionRequest request = new TransactionRequest
            {
                Amount = BraintreePaymentObject.PaymentAmount,
                MerchantAccountId = _braintreeConfiguration.GetConfigurationSetting("BraintreeMerchantAccountID"),
                PaymentMethodNonce = BraintreePaymentObject.Nonce,
                Customer = new CustomerRequest
                {
                    FirstName = BraintreePaymentObject.FirstName,
                    LastName = BraintreePaymentObject.LastName,
                    Phone = BraintreePaymentObject.PhoneNumber,
                    Email = BraintreePaymentObject.Email
                },
                BillingAddress = new AddressRequest
                {
                    FirstName = BraintreePaymentObject.FirstName,
                    LastName = BraintreePaymentObject.LastName,
                    StreetAddress = BraintreePaymentObject.Address,
                    Region = BraintreePaymentObject.Region,
                    Locality = BraintreePaymentObject.Locality,
                    PostalCode = BraintreePaymentObject.ZipCode,
                    CountryCodeAlpha2 = BraintreePaymentObject.CountryCode
                },
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
            };
            return request;
        }

        private string CreateErrorEmail(string ErrorMessage, EnrollmentFlowDto EnrollmentFlow)
        {
            StringBuilder sb = new StringBuilder(ErrorMessage);
            sb.Append(" | For Subscriber: " + EnrollmentFlow.SubscriberDto.SubscriberGuid);
            sb.Append(" | Enrollment: " + EnrollmentFlow.EnrollmentDto.EnrollmentGuid);
            return sb.ToString();
        }

        private string CreateResponseJson(string Response)
        {
            string ResponseJson = "{\"PaymentStatus\" : \"" + Response + "\"}";
            return ResponseJson;
        }

        /** This is copied from the CreateResponse method in the Woz Interface class.
         *  However, that method was specific to the Woz implementation.
         */
        private MessageTransactionResponse CreateResponse(string ResponseJson, string Info, string Data, TransactionState State)
        {
            MessageTransactionResponse RVal = new MessageTransactionResponse()
            {
                ResponseJson = ResponseJson,
                InformationalMessage = Info,
                Data = Data,
                State = State
            };
            return RVal;
        }
    }
}
