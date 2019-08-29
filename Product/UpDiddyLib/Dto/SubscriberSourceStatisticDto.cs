using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberSourceStatisticDto
    {
        public string Source { get; set; }
        public string Referrer { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
    }
}
