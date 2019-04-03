using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
   public class SubscriberActionSummaryDto
    {
        public string SubscriberEmail { get; set; }
        public string SubscriberFirstName { get; set; }
        public string SubscriberLastName { get; set; }
        public string Action { get; set; }
        public int ActionCount { get; set; }
    }
}
