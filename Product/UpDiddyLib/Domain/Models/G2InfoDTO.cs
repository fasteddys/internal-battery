using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class G2InfoDto
    {

        [JsonProperty("@search.score")]
        public double SearchScore { get; set; }


        public Guid ProfileGuid { get; set; }

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

        public bool IsWillingToTravel { get; set; }

        public bool IsActiveJobSeeker { get; set; }

        public bool IsCurrentlyEmployed { get; set; }

        public bool IsWillingToWorkProBono { get; set; }

        public float CurrentRate { get; set; }

        public float DesiredRate { get; set; }

        public string Tags { get; set; }

        public string Skills { get; set; }
        public string SearchLocations { get; set; }

 
        public int CompanyId { get; set; }

        public DateTime ModifyDate { get; set; }



    }
}
