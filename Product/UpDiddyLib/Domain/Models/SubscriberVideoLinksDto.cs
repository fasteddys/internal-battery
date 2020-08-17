using Newtonsoft.Json;
using System;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberVideoLinksDto
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid SubscriberVideoGuid { get; set; }

        public string VideoLink { get; set; }

        public string VideoMimeType { get; set; }

        public string ThumbnailLink { get; set; }

        public string ThumbnailMimeType { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? IsVisibleToHiringManager { get; set; }
    }
}
