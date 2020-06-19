using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceHiringManagerDto
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
