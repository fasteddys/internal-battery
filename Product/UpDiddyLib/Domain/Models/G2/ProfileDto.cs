using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models.G2
{
    public class ProfileListDto
    {
        public List<ProfileDto> ProfileDtos { get; set; } = new List<ProfileDto>();
        public int TotalRecords { get; set; }
    }

    public class ProfileDto
    {
        public Guid ProfileGuid { get; set; } // todo: do we need this to be exposed in the dto?
        public Guid CompanyGuid { get; set; } // todo: do we need this to be exposed in the dto?
        public Guid SubscriberGuid { get; set; }
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        [StringLength(254)]
        public string Email { get; set; }
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        public Guid? ContactGuid { get; set; }
        [StringLength(100)]
        public string StreetAddress { get; set; }
        public Guid? CityGuid { get; set; }
        public Guid? StateGuid { get; set; }
        public Guid? PostalGuid { get; set; }
        public Guid? ExperienceLevelGuid { get; set; }
        public Guid? EmploymentTypeGuid { get; set; }
        [StringLength(100)]
        public string Title { get; set; }
        public bool? IsWillingToTravel { get; set; }
        public bool? IsActiveJobSeeker { get; set; }
        public bool? IsCurrentlyEmployed { get; set; }
        public bool? IsWillingToWorkProBono { get; set; }
        public decimal? CurrentRate { get; set; }
        public decimal? DesiredRate { get; set; }
        [StringLength(500)]
        public string Goals { get; set; }
        [StringLength(500)]
        public string Preferences { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
