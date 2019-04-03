using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class BasicCountReportDto
    {
        public int SubscriberCount { get; set; }
        public int EnrollmentCount { get; set; }
        public string PartnerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
