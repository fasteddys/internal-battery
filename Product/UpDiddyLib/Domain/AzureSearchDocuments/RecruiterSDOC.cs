using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
 

namespace UpDiddyLib.Domain.AzureSearchDocuments
{


    public class RecruiterSDOC
    {
        [JsonProperty("@search.action")]
        public string SearchAction { get; set; }

 
        public Guid RecruiterGuid { get; set; }

        public Guid SubscriberGuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public Guid CompanyGuid { get; set; }

        public string CompanyName { get; set; }

    }
}
