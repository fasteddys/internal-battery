using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.Helpers;
using UpDiddyLib.Domain.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using SkillDto = UpDiddyLib.Domain.Models.SkillDto;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Workflow;
using EnrollmentStatus = UpDiddyApi.Models.EnrollmentStatus;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CourseEnrollmentService : ICourseEnrollmentService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private WozInterface _wozInterface = null;
        private ILogger _syslog;
        private UpDiddyDbContext _db;
        private readonly IHangfireService _hangfireService;
        private readonly IPromoCodeService _promoCodeService;





        public CourseEnrollmentService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration, ILogger<CourseEnrollmentService> syslog, UpDiddyDbContext db, IHttpClientFactory httpClientFactory, IHangfireService hangFireService, IPromoCodeService promoCodeService )
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = configuration;
            _wozInterface = new WozInterface(db, _mapper, configuration, syslog, httpClientFactory);
            _syslog = syslog;
            _db = db;
            _hangfireService = hangFireService;
            _promoCodeService = promoCodeService;
        }

        public async Task<Guid> Enroll(Guid subscriberGuid, CourseEnrollmentDto courseEnrollmentDto, string courseSlug)
        {

   
            Course course = _repositoryWrapper.Course.GetAll()
            .Include(c => c.Vendor)
            .Include(c => c.CourseVariants).ThenInclude(cv => cv.CourseVariantType)
            .Include(c => c.CourseSkills).ThenInclude(cs => cs.Skill)
            .Where(t => t.IsDeleted == 0 && t.Slug == courseSlug)
            .FirstOrDefault();

            if (course == null)
                throw new NotFoundException($"Course {courseSlug} does not exist");

            CourseVariant courseVariant = _repositoryWrapper.CourseVariant.GetAll()
            .Include(c => c.CourseVariantType) 
            .Where(t => t.IsDeleted == 0 && t.CourseVariantGuid == courseEnrollmentDto.CourseVariantGuid)
            .FirstOrDefault();


            if (courseVariant == null)
                throw new NotFoundException($"Course Variant {courseEnrollmentDto.CourseVariantGuid} does not exist");



            if (course.CourseGuid != courseEnrollmentDto.CourseGuid)
                throw new FailedValidationException($"Course enrollment information mis-aligned with course slug");

            if (courseEnrollmentDto == null)
                throw new FailedValidationException($"Course enrollment information not found");

            if (courseEnrollmentDto.SubscriberGuid == null)
                throw new FailedValidationException($"Subscriber must be specified for enrollment");

            if (subscriberGuid != courseEnrollmentDto.SubscriberGuid)
                throw new UnauthorizedAccessException("Logged in subscriber does not match subscriber being enrolled");


            // map the CourseEnrollmentDto to an EnrollmentFlowDto 
            EnrollmentFlowDto EnrollmentFlowDto = await CreateEnrollmentFlowDto(course,courseVariant, subscriberGuid, courseEnrollmentDto, courseSlug);
            //
            EnrollmentDto EnrollmentDto = EnrollmentFlowDto.EnrollmentDto;
            BraintreePaymentDto BraintreePaymentDto = EnrollmentFlowDto.BraintreePaymentDto;
             
            
            EnrollmentDto.Subscriber = null;
            Enrollment Enrollment = _mapper.Map<Enrollment>(EnrollmentDto);

            if (Enrollment.CampaignCourseVariant != null)
            {
                Enrollment.CampaignId = EnrollmentDto.CampaignCourseVariant.Campaign.CampaignId;
                Enrollment.CourseVariantId = EnrollmentDto.CampaignCourseVariant.CourseVariant.CourseVariantId;
                Enrollment.CampaignCourseVariant = null;
            }
            _repositoryWrapper.EnrollmentRepository.Create(Enrollment);

            /* todo: need to revisit the enrollment log for a number of reasons: 
             *      mismatch for "required" between properties of this and related entities (e.g. SubscriberGuid)
             *      what does EnrollmentTime mean, how does this differ from SectionStartTimestamp, DateEnrolled, or even just CreateDate
             *      does it make sense for PromoApplied to be required?
             *      redundancy between CourseVariantGuid and CourseCost - this may cause problems (e.g. CourseCost)
             *      currently payment status is fixed because we are processing the BrainTree payment synchronously; this will not be the case in the future
             *      the payment month and year is 30 days after the enrollment date for Woz, will be different for other vendors (forcing us to address that when we onboard the next vendor)
             */
            DateTime currentDate = DateTime.UtcNow; 
            var vendor = _repositoryWrapper.Vendor.GetAll().Where(v => v.VendorId == course.VendorId).FirstOrDefault(); // why is vendor id nullable on course?

            var originalCoursePrice = _repositoryWrapper.CourseVariant.GetAll()
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
                CourseGuid = EnrollmentDto.CourseGuid,
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

            // Rdeem the promocode if on has been applied
            _promoCodeService.Redeem(subscriberGuid, courseEnrollmentDto.PromoCodeRedemptionGuid, courseVariant.CourseVariantGuid.Value);

         
            /**
             *  This line used to enqueue the enrollment flow. Now, it's enqueuing the braintree flow,
             *  which will then enqueue the enrollment flow if the payment is successful.
             */
            _hangfireService.Enqueue<BraintreePaymentFlow>(x => x.PaymentWorkItem(EnrollmentFlowDto));

            return Enrollment.EnrollmentGuid.Value;
            // grab the subscriber information we need for the enrollment log, then set the property to null on EnrollmentDto so that ef doesn't try to create the subscriber
       
       

        }


        public async Task<CourseCheckoutInfoDto> GetCourseCheckoutInfo(Guid subscriberGuid, string courseSlug)
        {
        
            Course course = _repositoryWrapper.Course.GetAll()
                .Include(c => c.Vendor)
                .Include(c => c.CourseVariants).ThenInclude(cv => cv.CourseVariantType)
                .Include(c => c.CourseSkills).ThenInclude(cs => cs.Skill)
                .Where(t => t.IsDeleted == 0 && t.Slug == courseSlug)
                .FirstOrDefault();

            if (course == null)
                throw new NotFoundException($"Course {courseSlug} does not exist" );

            UpDiddyLib.Dto.SubscriberDto subscriberDto = await SubscriberFactory.GetSubscriber(_repositoryWrapper, subscriberGuid, _syslog, _mapper);
           
            // create course checkout and fill in course info
            CourseCheckoutInfoDto rVal = new CourseCheckoutInfoDto()
            {
                Name = course.Name,
                Code = course.Code,
                Slug = course.Slug,
                Description = course.Description,
                CourseGuid = course.CourseGuid.Value
            };

            // Fill in subscriber info
            rVal.FirstName = subscriberDto.FirstName;
            rVal.LastName = subscriberDto.LastName;
            rVal.PhoneNumber = subscriberDto.PhoneNumber;
            rVal.SubscriberGuid = subscriberDto.SubscriberGuid.Value;

            // if this is a woz course, get the terms of service and course schedule. 
            // todo: replace this logic with factory pattern when we add more vendors?
            List<DateTime> startDateUTCs = null;
            if (course.Vendor.Name == "WozU")
            {
                // get the terms of service from WozU
                var tos = _wozInterface.GetTermsOfService();
                rVal.TermsOfServiceId = tos.DocumentId;
                rVal.TermsOfService = tos.TermsOfService;

                // get start dates from WozU
                startDateUTCs = _wozInterface.CheckCourseSchedule(course.Code);
         
            }

            // not the greatest implementation performance-wise, but the alternative requires JOIN syntax and this is easier to read
            course.CourseSkills = course.CourseSkills.Where(cs => cs.IsDeleted == 0).ToList();
  
            // Get skills for course 
            rVal.Skills = new List<SkillDto>();
            foreach ( CourseSkill sk in course.CourseSkills)
            {
                rVal.Skills.Add(new SkillDto()
                {
                    Name = sk.Skill.SkillName,
                    SkillGuid = sk.Skill.SkillGuid.Value
                });

            }

            // Get the course variants for the course 
            course.CourseVariants = course.CourseVariants.Where(cv => cv.IsDeleted == 0).ToList();
            rVal.CourseVariants = new List<CourseVariantCheckoutDto>();
            foreach ( CourseVariant cv in course.CourseVariants)
            {
                CourseVariantCheckoutDto courseVariant = new CourseVariantCheckoutDto()
                {
                    CourseVariantGuid = cv.CourseVariantGuid.Value,
                    CourseVariantType = cv.CourseVariantType.Name,
                    Price = cv.Price,
                };

                courseVariant.IsElgibleCampaignOffer = subscriberDto.EligibleCampaigns.SelectMany(ec => ec.CampaignCourseVariant).Where(ccv => ccv.CourseVariant.CourseVariantGuid == courseVariant.CourseVariantGuid).Any();
                courseVariant.RebateOffer = subscriberDto.EligibleCampaigns.SelectMany(ec => ec.CampaignCourseVariant).Where(ccv => ccv.CourseVariant.CourseVariantGuid == courseVariant.CourseVariantGuid).FirstOrDefault()?.RebateType?.Description;
                courseVariant.RebateTerms = subscriberDto.EligibleCampaigns.SelectMany(ec => ec.CampaignCourseVariant).Where(ccv => ccv.CourseVariant.CourseVariantGuid == courseVariant.CourseVariantGuid).FirstOrDefault()?.RebateType?.Terms;
                var dates = startDateUTCs?.Select(i => new
                {
                    Key = i.ToShortDateString(),
                    Value = i.ToString()
                });
                courseVariant.StartDates = dates?.ToDictionary(i => i.Key, i => i.Value);
  
                // add course variant 
                rVal.CourseVariants.Add(courseVariant);
            }



            return rVal;
        }


        #region Private Helper functions

        private async Task<EnrollmentFlowDto> CreateEnrollmentFlowDto(Course course, CourseVariant courseVariant, Guid subscriberGuid, CourseEnrollmentDto courseEnrollmentDto, string courseSlug)
        {

            //  Create EnrollmentFlow DTO
            EnrollmentFlowDto EnrollmentFlowDto = new EnrollmentFlowDto();
            EnrollmentFlowDto.EnrollmentDto = new EnrollmentDto();

            EnrollmentFlowDto.SubscriberDto = await SubscriberFactory.GetSubscriber(_repositoryWrapper, courseEnrollmentDto.SubscriberGuid, _syslog, _mapper);

            if (EnrollmentFlowDto.SubscriberDto == null)
                throw new NotFoundException("Unable to locate subscriber");

            DateTime currentDate = DateTime.UtcNow;

            // Assign Enrollment information 
            EnrollmentFlowDto.EnrollmentDto.PricePaid = courseEnrollmentDto.PricePaid;
            EnrollmentFlowDto.EnrollmentDto.SubscriberId = EnrollmentFlowDto.SubscriberDto.SubscriberId;
            EnrollmentFlowDto.EnrollmentDto.EnrollmentGuid = Guid.NewGuid();
            EnrollmentFlowDto.EnrollmentDto.CourseId = course.CourseId;
            EnrollmentFlowDto.EnrollmentDto.CourseGuid = course.CourseGuid.Value;
            EnrollmentFlowDto.EnrollmentDto.SectionStartTimestamp = courseEnrollmentDto.SectionStartTimestamp;
            EnrollmentFlowDto.EnrollmentDto.CourseVariantGuid = courseVariant.CourseVariantGuid.Value;
            EnrollmentFlowDto.EnrollmentDto.CreateDate = currentDate;
            EnrollmentFlowDto.EnrollmentDto.ModifyDate = currentDate;
            EnrollmentFlowDto.EnrollmentDto.DateEnrolled = currentDate;
            EnrollmentFlowDto.EnrollmentDto.CreateGuid = Guid.Empty;
            EnrollmentFlowDto.EnrollmentDto.ModifyGuid = Guid.Empty;
            EnrollmentFlowDto.EnrollmentDto.PercentComplete = 0;
            EnrollmentFlowDto.EnrollmentDto.IsRetake = 0;
            EnrollmentFlowDto.EnrollmentDto.SectionStartTimestamp = courseEnrollmentDto.SectionStartTimestamp;
            EnrollmentFlowDto.EnrollmentDto.TermsOfServiceFlag = courseEnrollmentDto.TermsOfServiceId;
            EnrollmentFlowDto.EnrollmentDto.PromoCodeRedemptionGuid = courseEnrollmentDto.PromoCodeRedemptionGuid;

            // Set the enrollment status based on the course type
            switch (courseVariant.CourseVariantType.Name)
            {
                case "Instructor-Led":
                    EnrollmentFlowDto.EnrollmentDto.EnrollmentStatusId = (int)UpDiddyLib.Dto.EnrollmentStatus.FutureRegisterStudentRequested;
                    break;
                default:
                    EnrollmentFlowDto.EnrollmentDto.EnrollmentStatusId  = (int)UpDiddyLib.Dto.EnrollmentStatus.RegisterStudentRequested;
                    break;
            }
       
            // map braintree info 
            EnrollmentFlowDto.BraintreePaymentDto = _mapper.Map<BraintreePaymentDto>(courseEnrollmentDto);

            return EnrollmentFlowDto;
        }

        #endregion




    }
}
