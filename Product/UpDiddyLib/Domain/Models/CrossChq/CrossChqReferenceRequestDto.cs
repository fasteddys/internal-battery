using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class CrossChqReferenceRequestDto
    {
        public string CandidateMessage { get; set; }

        public string JobRole { get; set; }

        public string JobPosition { get; set; }

        [JsonProperty("configParams")]
        public CrossChqConfigurationParameters ConfigurationParameters { get; set; }
    }
}
