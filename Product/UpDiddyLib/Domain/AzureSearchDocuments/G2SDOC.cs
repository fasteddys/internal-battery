using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text; 
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using GeoJSON.Net.Geometry; 
 


namespace UpDiddyLib.Domain.AzureSearchDocuments
{
   public class G2SDOC
    {
        [JsonProperty("@search.action")]
        public string SearchAction { get; set; }
     
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

        //Note: When indexes are created it works without the new list.
        //we need the new list initialization because when we delete the indexes the code 
        //throws a null exception as azure json defines the null collection as an empty collection "[]".
        public List<string> EmploymentTypes { get; set; } = new List<string>(); 

        public string Title { get; set; }

        public bool? IsWillingToTravel { get; set; }

        public bool? IsActiveJobSeeker { get; set; }

        public bool? IsCurrentlyEmployed { get; set; }

        public bool? IsWillingToWorkProBono { get; set; }

        public bool? IsWillingToRelocate { get; set; }


        public float CurrentRate { get; set; }

        public float DesiredRate { get; set; }

        public string Tags { get; set; }

        public string PublicSkills { get; set; }

        public string PrivateSkills { get; set; }
    
        public Point Location { get; set; }

        public Guid CompanyGuid { get; set; }

        public DateTime? ModifyDate { get; set; }

        public Guid SubscriberGuid { get; set; }

        public string AvatarUrl { get; set; }

        public Guid PartnerGuid { get; set; }

        public DateTime? CreateDate { get; set; }

     
    }
}
