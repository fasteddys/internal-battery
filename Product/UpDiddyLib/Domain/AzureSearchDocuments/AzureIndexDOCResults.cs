using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.AzureSearchDocuments
{
    public class AzureIndexDOCResults
    {

        [JsonProperty("@odata.context")]
        public string dataContext { get; set; }
        public List<AzureIndexResultStatus> Value { get; set; }
    }



    public class AzureIndexResultStatus
    {
        public string Key { get; set; }
        public bool Status { get; set; }
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }
    }
}
