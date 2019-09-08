using UpDiddy.ViewModels.ButterCMS;
using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class ServiceCheckoutViewModel
    {
        public PackageServiceViewModel PackageServiceViewModel {get; set;}

        // these are used to populate dropdownlists for billing state and country
        public IEnumerable<SelectListItem> States { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Invalid email address. Please update your supplied email and try again.")]
        public string NewSubscriberEmail { get; set; }
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)|(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]))([A-Za-z\d@#$%^&*\-_+=[\]{}|\\:',?/`~""();!]|\.(?!@)){8,16}$", ErrorMessage = "Password must be 8-16 characters, containing 3 out of 4 of the following: Lowercase characters, uppercase characters, digits (0-9), and one or more of the following symbols: @ # $ % ^ & * - _ + = | ' , ? / ` ~ () ; .")]
        public string NewSubscriberPassword { get; set; }
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)|(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]))([A-Za-z\d@#$%^&*\-_+=[\]{}|\\:',?/`~""();!]|\.(?!@)){8,16}$", ErrorMessage = "Re-entered password must be 8-16 characters, containing 3 out of 4 of the following: Lowercase characters, uppercase characters, digits (0-9), and one or more of the following symbols: @ # $ % ^ & * - _ + = | ' , ? / ` ~ () ; .")]
        public string NewSubscriberReenterPassword { get; set; }
        public bool PackageAgreeToTermsAndConditions { get; set; }
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

        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Slug { get; set; }
        public int TermsOfServiceDocumentId { get; set; }
        public string TermsOfServiceContent { get; set; }

        // todo: implement IValidatableObject for model validation, should incorporate promo code redemption logic
        public Guid? PromoCodeRedemptionGuid { get; set; }
        public string PromoCodeEntered { get; set; }
    }
}

