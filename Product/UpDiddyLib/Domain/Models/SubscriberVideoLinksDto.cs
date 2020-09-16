using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberVideoLinksDto
    {
        public Guid SubscriberVideoGuid { get; set; }

        public string VideoLink { get; set; }

        public string VideoMimeType { get; set; }

        public string ThumbnailLink { get; set; }

        public string ThumbnailMimeType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SubscriberVideoStatus Status { get; set; }

        public bool? IsVisibleToHiringManager { get; set; }
    }

    public enum SubscriberVideoStatus
    {
        None = 0,
        Created,
        published
    }
}
