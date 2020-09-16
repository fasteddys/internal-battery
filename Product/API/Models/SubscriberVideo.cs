using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class SubscriberVideo : BaseModel
    {
        public int SubscriberVideoId { get; set; }

        public Guid SubscriberVideoGuid { get; set; }

        public int SubscriberId { get; set; }

        public virtual Subscriber Subscriber { get; set; }

        [StringLength(500)]
        public string VideoLink { get; set; }

        [StringLength(500)]
        public string VideoMimeType { get; set; }

        [StringLength(500)]
        public string ThumbnailLink { get; set; }

        [StringLength(500)]
        public string ThumbnailMimeType { get; set; }

        public bool IsPublished { get; internal set; }
    }
}
