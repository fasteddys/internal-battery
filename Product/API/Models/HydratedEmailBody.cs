using System;
using System.Collections.Generic;

namespace UpDiddyApi.Models
{
    public class HydratedEmailBody
    {
        public string Contents { get; set; }
        public Guid SubscriberGuid { get; set; }
        public Guid ProfileGuid { get; set; }

        public Dictionary<string, string> Tokens { get; set; } = new Dictionary<string, string>();
    }
}
