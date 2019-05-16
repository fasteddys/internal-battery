using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class JobPostingCountReportDto
    {
        public string CompanyName{ get; set; }
        public DateTime PostingDate { get; set; }
        public int PostingCount { get; set; }
    }
}
