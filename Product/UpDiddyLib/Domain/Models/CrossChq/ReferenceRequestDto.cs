using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceRequestDto
    {
        [JsonProperty("candidate")]
        public ReferenceCandidateDto Candidate { get; set; }

        [JsonProperty("jobrole")]
        public string JobRole { get; set; }

        [JsonProperty("requestor")]
        public string RequestorEmailAddress { get; set; }

        [JsonProperty("use_sms")]
        public bool UseSMS { get; set; }

        [JsonProperty("config_params")]
        public ConfigurationParametersDto ConfigurationParameters { get; set; }

        [JsonProperty("send_past_due_notification")]
        public bool SendPastDueNotification { get; set; }

        [JsonProperty("send_completed_notification")]
        public bool SendCompletedNotification { get; set; }

        [JsonProperty("candidate_message")]
        public string CandidateMessage { get; set; }

        [JsonProperty("job_position")]
        public string JobPosition { get; set; }

        [JsonProperty("hiring_manager")]
        public ReferenceHiringManagerDto HiringManager { get; set; }
    }
}
