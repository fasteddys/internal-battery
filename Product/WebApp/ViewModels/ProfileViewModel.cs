using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;


namespace UpDiddy.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        public List<EnrollmentDto> Enrollments { get; set; }
        public IEnumerable<SelectListItem> States { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
        public Guid? SelectedState { get; set; }
        public Guid? SelectedCountry { get; set; }
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "First name may not contain any special characters.")]
        public string FirstName { get; set; }
        [RegularExpression(@"^[a-zA-Z''-'\s]*$", ErrorMessage = "Last name may not contain any special characters.")]
        public string LastName { get; set; }
        [RegularExpression(@"^[0-9a-zA-Z''-'\s]*$", ErrorMessage = "Address may not contain any special characters.")]
        public string Address { get; set; }
        [RegularExpression(@"^[0-9a-zA-Z''-'\s]*$", ErrorMessage = "City may not contain any special characters.")]
        public string City { get; set; }
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter a 10-digit phone number.")]
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
                _FormattedPhone = value.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
            }
        }
        public string Phone
        {
            get
            {
                return this.FormattedPhone.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
            }
        }
        // no validation data annotation required for email since this has been validated by MSAL and cannot be modified by the user
        public string Email { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?facebook.com/[A-z0-9_]+/?$", ErrorMessage = "The Facebook profile URL is not valid.")]
        public string FacebookUrl { get; set; }       
        [RegularExpression(@"^http(s)?://([\w]+.)?linkedin.com/in/[A-z0-9_]+/?$", ErrorMessage = "The LinkedIn profile URL is not valid.")]
        public string LinkedInUrl { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?twitter.com/[A-z0-9_]+/?$", ErrorMessage = "The Twitter profile URL is not valid.")]
        public string TwitterUrl { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?stackoverflow.com/users/[0-9]+/[A-z0-9_]+/?$", ErrorMessage = "The StackOverflow profile URL is not valid.")]
        public string StackOverflowUrl { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?github.com/[A-z0-9_]+/?$", ErrorMessage = "The GitHub profile URL is not valid.")]
        public string GithubUrl { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public Boolean IsAnyProfileInformationPopulated
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(this.FirstName) ||
                    !string.IsNullOrWhiteSpace(this.LastName) ||
                    !string.IsNullOrWhiteSpace(this.Address) ||
                    !string.IsNullOrWhiteSpace(this.City) ||
                    !string.IsNullOrWhiteSpace(this.Phone) ||
                    !string.IsNullOrWhiteSpace(this.FacebookUrl) ||
                    !string.IsNullOrWhiteSpace(this.LinkedInUrl) ||
                    !string.IsNullOrWhiteSpace(this.TwitterUrl) ||
                    !string.IsNullOrWhiteSpace(this.StackOverflowUrl) ||
                    !string.IsNullOrWhiteSpace(this.GithubUrl) ||
                    SelectedState.HasValue;
            }
        }
    }
}
