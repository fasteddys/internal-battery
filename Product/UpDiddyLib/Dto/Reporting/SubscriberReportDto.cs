using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class SubscriberReportDto
    {
        public BasicCountReportDto Totals { get; set; }
        public List<BasicCountReportDto> Report { get; set; }
    }
}
