using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceRequest
    {
        [JsonProperty("candidate")]
        public ReferenceCandidate Candidate { get; set; }

        [JsonProperty("jobrole")]
        public string JobRole { get; set; }

        [JsonProperty("requestor")]
        public string RequestorEmailAddress { get; set; }

        [JsonProperty("use_sms")]
        public bool UseSMS { get; set; }

        [JsonProperty("config_params")]
        public ReferenceRequestConfigurationParameters ConfigurationParameters { get; set; }

        [JsonProperty("send_past_due_notification")]
        public bool SendPastDueNotification { get; set; }

        [JsonProperty("send_completed_notification")]
        public bool SendCompletedNotification { get; set; }

        [JsonProperty("candidate_message")]
        public string CandidateMessage { get; set; }

        [JsonProperty("job_position")]
        public string JobPosition { get; set; }

        [JsonProperty("hiring_manager")]
        public ReferenceHiringManager HiringManager { get; set; }
    }

    public class ReferenceRequestConfigurationParameters
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
