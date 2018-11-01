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
        public SubscriberDto Subscriber { get; set; }
        public Dictionary<CountryDto, List<StateDto>> CountryStateMapping { get; set; }
        public IList<WozCourseProgress> CurrentEnrollments { get; set; }
        public CountryDto Country { get; set; }
        public StateDto State { get; set; }
        public Boolean HasAnyInformationToDisplay {get; set;}

        // Strings for display, placeholder, and updating purposes.
        public string DisplayedFirstName { get; set; }
        public string FirstNamePlaceholder { get; set; }
        public string DisplayedLastName { get; set; }
        public string LastNamePlaceholder { get; set; }
        public string DisplayedAddress { get; set; }
        public string AddressPlaceholder { get; set; }
        public string DisplayedCity { get; set; }
        public string CityPlaceholder { get; set; }
        public string DisplayedState { get; set; }
        public string StatePlaceholder { get; set; }
        public string DisplayedCountry { get; set; }
        public string CountryPlaceholder { get; set; }
        public string DisplayedPhone { get; set; }
        public string PhonePlaceholder { get; set; }
        public string DisplayedFacebookUrl { get; set; }
        public string FacebookUrlPlaceholder { get; set; }
        public string DisplayedLinkedInUrl { get; set; }
        public string LinkedInUrlPlaceholder { get; set; }
        public string DisplayedTwitterUrl { get; set; }
        public string TwitterUrlPlaceholder { get; set; }
        public string DisplayedStackOverflowUrl { get; set; }
        public string StackOverflowUrlPlaceholder { get; set; }
        public string DisplayedGithubUrl { get; set; }
        public string GithubUrlPlaceholder { get; set; }
        public string UpdatedFirstName { get; set; }
        public string UpdatedLastName { get; set; }
        public string UpdatedAddress { get; set; }
        public string UpdatedCity { get; set; }
        public string UpdatedState { get; set; }
        public string UpdatedCountry { get; set; }
        public string UpdatedPhoneNumber { get; set; }
        public string UpdatedFacebookUrl { get; set; }
        public string UpdatedLinkedInUrl { get; set; }
        public string UpdatedTwitterUrl { get; set; }
        public string UpdatedStackOverflowUrl { get; set; }
        public string UpdatedGithubUrl { get; set; }
        public Guid CurrentSubscriberGuid { get; set; }

        public ProfileViewModel(
            IConfiguration _configuration, 
            SubscriberDto subscriber, 
            IList<CountryStateDto> CountryStateList,
            IList<WozCourseProgress> WozCourseProgressions,
            CountryDto SubscriberCountry,
            StateDto SubscriberState)
        {
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Subscriber = subscriber;
            this.CurrentEnrollments = WozCourseProgressions;
            this.Country = SubscriberCountry;
            this.State = SubscriberState;
            SetProfileDisplayValues();
            SetInformationFlag();
            this.CountryStateMapping = Utils.InitializeCountryStateMapping(CountryStateList);
            //InitializeCountryStateMapping(CountryStateList);
        }

        private void SetProfileDisplayValues()
        {
            this.DisplayedFirstName = string.IsNullOrEmpty(this.Subscriber.FirstName) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.FirstName;
            this.FirstNamePlaceholder = string.IsNullOrEmpty(this.Subscriber.FirstName) ? "First Name" : this.Subscriber.FirstName;

            this.DisplayedLastName = string.IsNullOrEmpty(this.Subscriber.LastName) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.LastName;
            this.LastNamePlaceholder = string.IsNullOrEmpty(this.Subscriber.LastName) ? "Last Name" : this.Subscriber.LastName;

            this.DisplayedAddress = string.IsNullOrEmpty(this.Subscriber.Address) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.Address;
            this.AddressPlaceholder = string.IsNullOrEmpty(this.Subscriber.Address) ? "Address" : this.Subscriber.Address;
            
            this.DisplayedCity = string.IsNullOrEmpty(this.Subscriber.City) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.City;
            this.CityPlaceholder = string.IsNullOrEmpty(this.Subscriber.City) ? "City" : this.Subscriber.City;

            this.DisplayedState = string.IsNullOrEmpty(this.State.Name) ? Helpers.Constants.EMPTY_STRING : this.State.Name;
            this.DisplayedCountry = string.IsNullOrEmpty(this.Country.DisplayName) ? Helpers.Constants.EMPTY_STRING : this.Country.DisplayName;

            this.DisplayedPhone = string.IsNullOrEmpty(this.Subscriber.PhoneNumber) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.PhoneNumber;
            this.PhonePlaceholder = string.IsNullOrEmpty(this.Subscriber.PhoneNumber) ? "Phone Number" : this.Subscriber.PhoneNumber;

            this.DisplayedFacebookUrl = string.IsNullOrEmpty(this.Subscriber.FacebookUrl) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.FacebookUrl;
            this.FacebookUrlPlaceholder = string.IsNullOrEmpty(this.Subscriber.FacebookUrl) ? "Facebook Profile" : this.Subscriber.FacebookUrl;

            this.DisplayedLinkedInUrl = string.IsNullOrEmpty(this.Subscriber.LinkedInUrl) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.LinkedInUrl;
            this.LinkedInUrlPlaceholder = string.IsNullOrEmpty(this.Subscriber.LinkedInUrl) ? "LinkedIn Profile" : this.Subscriber.LinkedInUrl;

            this.DisplayedTwitterUrl = string.IsNullOrEmpty(this.Subscriber.TwitterUrl) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.TwitterUrl;
            this.TwitterUrlPlaceholder = string.IsNullOrEmpty(this.Subscriber.TwitterUrl) ? "Twitter Profile" : this.Subscriber.TwitterUrl;

            this.DisplayedStackOverflowUrl = string.IsNullOrEmpty(this.Subscriber.StackOverflowUrl) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.StackOverflowUrl;
            this.StackOverflowUrlPlaceholder = string.IsNullOrEmpty(this.Subscriber.StackOverflowUrl) ? "StackOverflow Profile" : this.Subscriber.StackOverflowUrl;

            this.DisplayedGithubUrl = string.IsNullOrEmpty(this.Subscriber.GithubUrl) ? Helpers.Constants.EMPTY_STRING : this.Subscriber.GithubUrl;
            this.GithubUrlPlaceholder = string.IsNullOrEmpty(this.Subscriber.GithubUrl) ? "GitHub Profile" : this.Subscriber.GithubUrl;
        }

        private void SetInformationFlag()
        {
            if(!string.IsNullOrEmpty(this.DisplayedFirstName) ||
                !string.IsNullOrEmpty(this.DisplayedLastName) ||
                !string.IsNullOrEmpty(this.DisplayedAddress) ||
                !string.IsNullOrEmpty(this.DisplayedCity) ||
                !string.IsNullOrEmpty(this.DisplayedState) ||
                !string.IsNullOrEmpty(this.DisplayedCountry) ||
                !string.IsNullOrEmpty(this.DisplayedPhone) ||
                !string.IsNullOrEmpty(this.DisplayedFacebookUrl) ||
                !string.IsNullOrEmpty(this.DisplayedTwitterUrl) ||
                !string.IsNullOrEmpty(this.DisplayedLinkedInUrl) ||
                !string.IsNullOrEmpty(this.DisplayedStackOverflowUrl) ||
                !string.IsNullOrEmpty(this.DisplayedGithubUrl))
            {
                this.HasAnyInformationToDisplay = true;
            }
            else
            {
                this.HasAnyInformationToDisplay = false;
            }
        }

        private void InitializeCountryStateMapping(IList<CountryStateDto> CountryStateList)
        {
            /*
            string previousCountry = CountryStateList[0].DisplayName;
            List<StateDto> states = new List<StateDto>();
            this.CountryStateMapping = new Dictionary<string, List<StateDto>>();
            foreach (CountryStateDto csdto in CountryStateList)
            {
                if (!(previousCountry).Equals(csdto.DisplayName))
                {
                    this.CountryStateMapping.Add(previousCountry, states);
                    states = new List<StateDto>();
                    previousCountry = csdto.DisplayName;
                    states.Add(new StateDto {
                        Name = csdto.Name,
                        StateId = csdto.StateId
                    });
                }
                else
                {
                    states.Add(new StateDto
                    {
                        Name = csdto.Name,
                        StateId = csdto.StateId
                    });
                }
            }
            */
        }
    }
}
