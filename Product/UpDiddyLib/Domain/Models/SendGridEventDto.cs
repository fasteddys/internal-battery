using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Dto;

namespace UpDiddyLib.Domain.Models
{
    public class SendGridEventDto :  BaseDto
    {
        public string Email { get; set; }
        public long Timestamp { get; set; }

        public string Smtp_id   {get; set;} 

        public string Event { get; set; }

        public string Category { get; set; }

        public string Sg_event_id { get; set; }

        public string Sg_message_id { get; set; }

        public string Response { get; set; }

        public string Attempt { get; set; }

        public string UserAgent { get; set; }

        public string Ip { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }

        public string Tls { get; set; }

        public string Url { get; set; }

        public string Type { get; set; }

        public string Marketing_campaign_id { get; set; }

        public string Marketing_campaign_name { get; set; }

        public string Subject { get; set; }



    }
}
