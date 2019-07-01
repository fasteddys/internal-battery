using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberNotificationDto : BaseDto
    {
        public int SubscriberNotificationId { get; set; }
        public Guid SubscriberNotificationGuid { get; set; }
        public int SubscriberId { get; set; }
        public int NotificationId { get; set; }
        public bool HasRead { get; set; }
    }
}
