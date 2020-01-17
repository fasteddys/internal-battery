using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberSendGridEvent  : BaseModel
    {
        public int SubscriberSendGridEventId { get; set; }
        public Guid SubscriberSendGridEventGuid { get; set; }


        public int? SubscriberId { get; set; }

        public string Event { get; set; }
        public string Category { get; set; }

        public int EventStatus { get; set; }

        public string Type { get; set; }
        public string Marketing_campaign_id { get; set; }

        public string Marketing_campaign_name { get; set; }

        public string Subject { get; set; }

        public string Sg_message_id { get; set; }

        public string Response { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }
        public string Attempt { get; set; }

        public string Email { get; set; }





    }
}
