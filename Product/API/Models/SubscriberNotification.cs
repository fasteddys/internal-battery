using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberNotification : BaseModel
    {
        public int SubscriberNotificationId { get; set; }
        public Guid SubscriberNotificationGuid { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int NotificationId { get; set; }
        public virtual Notification Notification { get; set; }
        public bool HasRead { get; set; }

    }
}
