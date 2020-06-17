using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class SubscriberLink : BaseModel
    {
        public int SubscriberLinkId { get; set; }
        public Guid SubscriberLinkGuid { get; set; }
        [Required, StringLength(2000)]
        public string Url { get; set; }
        [StringLength(100)]
        public string Label { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
    }
}
