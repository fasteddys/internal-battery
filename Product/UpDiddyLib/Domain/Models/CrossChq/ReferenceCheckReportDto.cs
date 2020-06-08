using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceCheckReportDto
    {
        public Guid ReferenceCheckGuid { get; set; }
        public string ReportFileUrl { get; set; }

        /// <summary>
        /// Hardcoded report type - values: Full/Summary. 
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// The Base64 string of the report pdf file.
        /// </summary>
        public string ReportFile { get; set; }
    }
}
