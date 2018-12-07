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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public StateDto State { get; set; }
        // todo: put this here for the DDL on front-end, but don't think it will be used. remove?
        public CountryDto Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string StackOverflowUrl { get; set; }
        public string GithubUrl { get; set; }
        public Guid? SubscriberGuid { get; set; }
        // do we need this?
        public Boolean IsAnyProfileInformationPopulated
        {
            get
            {
                return
                    string.IsNullOrWhiteSpace(this.FirstName) &&
                    string.IsNullOrWhiteSpace(this.LastName) &&
                    string.IsNullOrWhiteSpace(this.Address) &&
                    string.IsNullOrWhiteSpace(this.City) &&
                    string.IsNullOrWhiteSpace(this.Phone) &&
                    string.IsNullOrWhiteSpace(this.FacebookUrl) &&
                    string.IsNullOrWhiteSpace(this.LinkedInUrl) &&
                    string.IsNullOrWhiteSpace(this.TwitterUrl) &&
                    string.IsNullOrWhiteSpace(this.StackOverflowUrl) &&
                    string.IsNullOrWhiteSpace(this.GithubUrl) &&
                    (State==null);
            }
        }
    }
}
