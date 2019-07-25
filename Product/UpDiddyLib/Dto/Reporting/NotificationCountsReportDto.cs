using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class NotificationCountsReportDto
    {
        public string NotificationTitle { get; set; }
        public DateTime PublishedDate { get; set; }
        public int ReadCount { get; set; }
    }
}
