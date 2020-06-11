using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceCandidateDto
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobile_phone", DefaultValueHandling = DefaultValueHandling.Ignore)] // omits this property if the value is null
        public string MobilePhone { get; set; }
    }
}
