using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class SocialLinksDto
    {
        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
