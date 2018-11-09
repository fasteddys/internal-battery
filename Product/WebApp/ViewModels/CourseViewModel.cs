using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class CourseViewModel
    {
        public StateViewModel SelectedState { get; set; }
        public IEnumerable<SelectListItem> States {get;set;}
        public CountryViewModel SelectedCountry { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }

        public Guid SubscriberGuid { get; set; }
        public string SubscriberFirstName { get; set; }
        public string SubscriberLastName { get; set; }

        [Required]
        public CourseVariantViewModel SelectedCourseVariant { get; set; }
        public IEnumerable<CourseVariantViewModel> CourseVariants { get; set; }
        public Guid CourseGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public int TermsOfServiceDocumentId { get; set; }
        public string TermsOfServiceContent { get; set; }

        public CourseDto Course { get; set; }


        public Dictionary<CountryDto, List<StateDto>> CountryStateMapping { get; set; }
        public string PaymentMethodNonce { get; set; }
        public string BillingFirstName { get; set; }
        public string BillingLastName { get; set; }
        public string BillingState { get; set; }
        public string BillingCity { get; set; }
        public string BillingZipCode { get; set; }
        public string BillingCountry { get; set; }
        public string BillingAddress { get; set; }
        //todo: implement this convenient functionality after profile info has been refactored
        public Boolean SameAsAboveCheckbox { get; set; }
        public Guid? PromoCodeRedemptionGuid { get; set; }
        // todo: refactor enrollment to accept coursevariantid instead of courseid / courseguid
    }

    public class CourseVariantViewModel
    {
        public DateTime? SelectedStartDate { get; set; }
        public IEnumerable<SelectListItem> StartDates { get; set; }
        public Guid CourseVariantGuid { get; set; }
        public Decimal Price { get; set; }
        public string CourseVariantType { get; set; }
    }
}
