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
        public string PromoCodeForSubmission { get; set; }
        public CourseViewModel(IConfiguration _configuration, CourseDto course, SubscriberDto subscriber, TopicDto parentTopic, WozTermsOfServiceDto tos)
        {
            this.TermsOfService = tos;
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Parent = parentTopic;
            this.Subscriber = subscriber;
            this.Course = course;
        }
    }
}
