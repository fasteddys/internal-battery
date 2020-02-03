using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Domain.AzureSearchDocuments;

namespace UpDiddyLib.Domain.AzureSearch
{

    public class SubscriberSDOC
    {
        [JsonProperty("@search.action")]
        public string SearchAction { get; set; }

        public Guid SubscriberGuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set;}

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

    }
}
