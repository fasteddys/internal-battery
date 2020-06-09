using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceCandidate : ReferenceHiringManager
    {
        [JsonProperty("mobile_phone", DefaultValueHandling = DefaultValueHandling.Ignore)] // omits this property if the value is null
        public string MobilePhone { get; set; }
    }

    public class ReferenceHiringManager
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class Reference : ReferenceCandidate
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
