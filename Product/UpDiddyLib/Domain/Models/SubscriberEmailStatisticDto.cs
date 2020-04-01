using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    
    public class SubscriberEmailStatisticDto
    {
        public string Email { get; set; }
        public string Event { get; set; }
        public DateTime LatestEventDate { get; set; }
        public string LatestEmailSubject { get; set; }
        public int NumEvents { get; set; }
    }
}
