using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class CourseViewModel : BaseViewModel
    {
        public TopicDto Parent { get; set; }
        public CourseDto Course { get; set; }
        public SubscriberDto Subscriber { get; set; }
        public WozTermsOfServiceDto TermsOfService { get; set; }
        public WozCourseScheduleDto WozCourseSchedule { get; set; }
        public Boolean IsInstructorLed { get; set; }
        public Decimal SelfPacedPrice { get; set; }
        public Decimal InstructorLedPrice { get; set; }
        public Boolean TermsOfServiceDocId { get; set; }
        public string CourseSlug { get; set; }
        public string PaymentMethodNonce { get; set; }
        public string BillingFirstName { get; set; }
        public string BillingLastName { get; set; }
        public string BillingState { get; set; }
        public string BillingCity { get; set; }
        public string BillingZipCode { get; set; }
        public string BillingCountry { get; set; }
        public string BillingAddress { get; set; }
        public Boolean SameAsAboveCheckbox { get; set; }
        public Guid PromoCodeRedemptionGuid { get; set; }
        public Boolean InstructorLedChosen { get; set; }
        public Boolean SelfPacedChosen { get; set; }
        public int? SelfPacedCourseVariantId { get; set; }
        public int? InstructorLedCourseVariantId { get; set; }
        public Int64 DateOfInstructorLedSection { get; set; }

        public CourseViewModel(
            IConfiguration _configuration,
            CourseDto course,
            SubscriberDto subscriber,
            TopicDto parentTopic,
            WozTermsOfServiceDto tos,
            WozCourseScheduleDto wcsdto)
        {
            this.TermsOfService = tos;
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Parent = parentTopic;
            this.Subscriber = subscriber;
            this.Course = course;
            this.WozCourseSchedule = wcsdto;
            // todo: refactor this after go-live
            foreach (Tuple<int, string, decimal> tuple in wcsdto.VariantToPrice)
            {
                switch (tuple.Item2)
                {
                    case "selfpaced":
                        this.SelfPacedPrice = tuple.Item3;
                        this.SelfPacedCourseVariantId = tuple.Item1;
                        break;
                    case "instructor":
                        this.InstructorLedPrice = tuple.Item3;
                        this.InstructorLedCourseVariantId = tuple.Item1;
                        break;
                }
            }
            this.IsInstructorLed = wcsdto.StartDatesUTC.Count > 0;
        }
    }
}
