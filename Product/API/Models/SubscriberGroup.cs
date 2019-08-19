using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberGroup : BaseModel
    {
        public int SubscriberGroupId { get; set; }
        public Guid SubscriberGroupGuid { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int SubscriberId { get; set; }
        public virtual Group Group { get; set; }
        public int GroupId { get; set; }
    }
}
