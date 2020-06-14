using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ConfigurationParametersDto
    {
        [JsonProperty("managers")]
        public int Managers { get; set; }

        [JsonProperty("employees")]
        public int Employees { get; set; }

        [JsonProperty("peers")]
        public int Peers { get; set; }

        [JsonProperty("business")]
        public int Business { get; set; }

        [JsonProperty("social")]
        public int Social { get; set; }
    }
}
