using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.MessageQueue
{
    public class EnrollmentMessage : QueueMessage
    {
        public double Nonce { get; set; }
        public string CourseCode { get; set; }
        public string VendorGuid { get; set; }
        public string SubscriberGuid { get; set; }
    }
}
