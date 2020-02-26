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

   
        public Point Location { get; set; }

        public int CompanyId { get; set; }

        public DateTime ModifyDate { get; set; }
 
    }
}
