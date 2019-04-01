using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class OfferActionSummaryDto
    {
        public string OfferName { get; set; }
        public string OfferCode { get; set; }
        public string Action { get; set; }
        public int ActionCount { get; set; }
    }
}