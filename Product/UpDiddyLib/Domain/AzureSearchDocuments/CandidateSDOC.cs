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
   public class CandidateSDOC
    {
      
        [JsonProperty("@search.action")]
        public string SearchAction { get; set; }
      
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Postal { get; set; }
 
        public string Title { get; set; }
         
        public float CurrentRate { get; set; }
                
        public Point Location { get; set; }
 
        public DateTime? ModifyDate { get; set; }

        public Guid SubscriberGuid { get; set; }

        public string AvatarUrl { get; set; }

        public Guid PartnerGuid { get; set; }

        public DateTime? CreateDate { get; set; }

        public List<string> Skills { get; set; }

        public List<LanguageSDOC> Languages { get; set; }

    }
}
