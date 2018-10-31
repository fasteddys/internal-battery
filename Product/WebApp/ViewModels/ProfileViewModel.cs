using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        public string UpdatedFirstName { get; set; }
        public string UpdatedLastName { get; set; }
        public string UpdatedAddress { get; set; }
        public string UpdatedCity { get; set; }
        public string UpdatedState { get; set; }
        public string UpdatedCountry { get; set; }
        public string UpdatedPhoneNumber { get; set; }
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
            this.CountryStateMapping = Utils.InitializeCountryStateMapping(CountryStateList);
            //InitializeCountryStateMapping(CountryStateList);
        }

        private void SetProfileDisplayValues()
        {
            this.DisplayedFirstName = string.IsNullOrEmpty(this.Subscriber.FirstName) ? Helpers.Constants.NONE : this.Subscriber.FirstName;
            this.FirstNamePlaceholder = string.IsNullOrEmpty(this.Subscriber.FirstName) ? "First Name" : this.Subscriber.FirstName;

            this.DisplayedLastName = string.IsNullOrEmpty(this.Subscriber.LastName) ? Helpers.Constants.NONE : this.Subscriber.LastName;
            this.LastNamePlaceholder = string.IsNullOrEmpty(this.Subscriber.LastName) ? "Last Name" : this.Subscriber.LastName;

            this.DisplayedAddress = string.IsNullOrEmpty(this.Subscriber.Address) ? Helpers.Constants.NONE : this.Subscriber.Address;
            this.AddressPlaceholder = string.IsNullOrEmpty(this.Subscriber.Address) ? "Address" : this.Subscriber.Address;
            
            this.DisplayedCity = string.IsNullOrEmpty(this.Subscriber.City) ? Helpers.Constants.NONE : this.Subscriber.City;
            this.CityPlaceholder = string.IsNullOrEmpty(this.Subscriber.City) ? "City" : this.Subscriber.City;

            this.DisplayedState = string.IsNullOrEmpty(this.State.Name) ? Helpers.Constants.NONE : this.State.Name;

            this.DisplayedCountry = string.IsNullOrEmpty(this.Country.DisplayName) ? Helpers.Constants.NONE : this.Country.DisplayName;

            this.DisplayedPhone = string.IsNullOrEmpty(this.Subscriber.PhoneNumber) ? Helpers.Constants.NONE : this.Subscriber.PhoneNumber;
            this.PhonePlaceholder = string.IsNullOrEmpty(this.Subscriber.PhoneNumber) ? "Phone Number" : this.Subscriber.PhoneNumber;
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
