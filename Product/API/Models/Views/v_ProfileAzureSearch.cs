using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.Views
{
    [NotMapped]
    public class v_ProfileAzureSearch
    {
        public Guid ProfileGuid { get; set; }
        public Guid CompanyGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ContactType { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal { get; set; }
        public string ExperienceLevel { get; set; }
        public string EmploymentType { get; set; }
        public string Title { get; set; }
        public bool? IsWillingToTravel { get; set; }
        public bool? IsActiveJobSeeker { get; set; }
        public bool? IsCurrentlyEmployed { get; set; }
        public bool? IsWillingToWorkProBono { get; set; }
        public decimal? CurrentRate { get; set; }
        public decimal? DesiredRate { get; set; }
        public string PrivateSkills { get; set; }
        public string PublicSkills { get; set; }
        public string Tags { get; set; }
    }
}