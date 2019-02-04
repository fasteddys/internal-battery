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
        // these are used to populate dropdownlists for billing state and country
        public IEnumerable<SelectListItem> States { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }

        // billing information
        public string PaymentMethodNonce { get; set; }

        [Required(ErrorMessage = "A first name must be entered in billing information.")]
        public string BillingFirstName { get; set; }
        [Required(ErrorMessage = "A last name must be entered in billing information.")]
        public string BillingLastName { get; set; }
        [Required(ErrorMessage = "A street address must be entered in billing information.")]
        public string BillingAddress { get; set; }
        [Required(ErrorMessage = "A city must be entered in billing information.")]
        public string BillingCity { get; set; }
        [Required(ErrorMessage = "A state must be selected in billing information.")]
        public Guid SelectedState { get; set; }
        [Required(ErrorMessage = "A country must be selected in billing information.")]
        public Guid SelectedCountry { get; set; }
        [Required(ErrorMessage = "A zip code must be entered in billing information.")]
        public string BillingZipCode { get; set; }
        //todo: implement this convenient functionality after profile info has been refactored
        public Boolean SameAsAboveCheckbox { get; set; }

        public Guid SubscriberGuid { get; set; }

        [Required(ErrorMessage = "A first name must be entered for the subscriber.")]
        public string SubscriberFirstName { get; set; }
        [Required(ErrorMessage = "A last name must be entered for the subscriber.")]
        public string SubscriberLastName { get; set; }
        private string _FormattedPhone;
        public string FormattedPhone
        {
            get
            {
                long phone = 0;
                if (long.TryParse(this._FormattedPhone, out phone))
                    return String.Format("{0:(###) ###-####}", phone);
                else
                    return _FormattedPhone;
            }
            set
            {
                if (value != null)
                    _FormattedPhone = value.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                else
                    _FormattedPhone = value;
            }
        }
        [RegularExpression("^[2-9]{1}[0-9]{9}$", ErrorMessage = "Phone must be 10 digits and may not start with a 0 or 1. Please edit your phone number before continuing.")]
        public string SubscriberPhoneNumber
        {
            get
            {
                if (this._FormattedPhone != null)
                    return this.FormattedPhone.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                else
                    return _FormattedPhone;
            }
        }
        [Required(ErrorMessage = "A course section must be selected.")]
        public Guid? SelectedCourseVariant { get; set; }
        public DateTime? SelectedStartDate { get; set; }

        public IEnumerable<CourseVariantViewModel> CourseVariants { get; set; }
        public Guid CourseGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Slug { get; set; }
        public int TermsOfServiceDocumentId { get; set; }
        public string TermsOfServiceContent { get; set; }

        // this is populated based on SelectedCourseVariant after the form submission - may not need this...
        public CourseVariantViewModel CourseVariant { get; set; }

        // todo: implement IValidatableObject for model validation, should incorporate promo code redemption logic
        public Guid? PromoCodeRedemptionGuid { get; set; }
        public List<SkillDto> Skills { get; set; }
    }

    public class CourseVariantViewModel
    {
        public DateTime? SelectedStartDate { get; set; }
        public IEnumerable<SelectListItem> StartDates { get; set; }
        public Guid CourseVariantGuid { get; set; }
        public Decimal Price { get; set; }
        public string CourseVariantType { get; set; }
        public bool IsEligibleCampaignOffer { get; set; }
        public string RebateOffer { get; set; }
    }
}
