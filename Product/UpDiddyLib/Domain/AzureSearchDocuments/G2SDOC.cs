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



   

        public Point Location { get; set; }

        public string City { get; set; }

        public string Id { get; set; }
 
    }
}
