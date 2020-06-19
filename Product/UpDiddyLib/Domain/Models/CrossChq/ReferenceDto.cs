using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceDto
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobile_phone")]
        public string MobilePhone { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
