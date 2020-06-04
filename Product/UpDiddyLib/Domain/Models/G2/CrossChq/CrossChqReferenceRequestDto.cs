using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.G2.CrossChq
{
    public class CrossChqReferenceRequestDto
    {
        public string JobRole { get; set; }

        public string JobPosition { get; set; }

        [JsonProperty("configParams")]
        public CrossChqConfigurationParameters ConfigurationParameters { get; set; }
    }
}
