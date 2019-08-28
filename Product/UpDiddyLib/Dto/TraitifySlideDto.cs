using System;
using Newtonsoft.Json;

namespace UpDiddyLib.Dto
{
    public class TraitifySlideDto
    {
        [JsonProperty]
        public string id { get; set; }
        [JsonProperty]
        public int? position { get; set; }
        [JsonProperty]
        public string caption { get; set; }
        [JsonProperty]
        public string image_desktop { get; set; }
        [JsonProperty]
        public string image_desktop_retina { get; set; }
        [JsonProperty]
        public string image_phone_landscape { get; set; }
        [JsonProperty]
        public string image_phone_portrait { get; set; }
        [JsonProperty]
        public bool? response { get; set; }
        [JsonProperty]
        public long? time_taken { get; set; }
        [JsonProperty]
        public long? completed_at { get; set; }
        [JsonProperty]
        public long? created_at { get; set; }
        public DateTime? TimeTaken { get; }
        public DateTime? CompletedAt { get; }
        public DateTime CreatedAt { get; }
    }
}