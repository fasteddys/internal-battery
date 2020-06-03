using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CrosschqWebhookDto
    {
        public string Id { get; set; }
        public CrosschqCandidateDto Candidate { get; set; }
        public string Job_Role { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
        public List<CrosschqReferenceDto> References { get; set; }
        public string Created_At { get; set; }
        public string Updated_On { get; set; }

        /// <summary>
        /// Url to the report
        /// </summary>
        public string Report_Url { get; set; }

        /// <summary>
        /// Base64 string representation of summary report pdf file.
        /// </summary>
        public string Report_Summary_Pdf { get; set; }

        /// <summary>
        /// Base64 string representation of full report pdf file.
        /// </summary>
        public string Report_Full_Pdf { get; set; }

    }
}
