using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CommunicationSubscription : BaseModel
    {
        public int CommunicationSubscriptionId { get; set; }
        public Guid? CommunicationSubscriptionGuid { get; set; }
        public int SubscriberId { get; set; }
        public int CommunicationTypeId { get; set; }
        public DateTime SubscribeDate { get; set; }
    }
}
