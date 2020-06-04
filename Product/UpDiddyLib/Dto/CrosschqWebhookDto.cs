using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CrosschqWebhookDto
    {
        /// <summary>
        /// The Crosschq request_id.
        /// </summary>
        public string Id { get; set; }
        public CrosschqCandidateDto Candidate { get; set; }
        public string Job_Role { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
        public List<CrosschqReferenceDto> References { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_On { get; set; }
        public DateTime? Status_Updated_On { get; set; }

        /// <summary>
        /// Url to the report
        /// </summary>
        public string Report_Url { get; set; }

        /// <summary>
        /// Url to a summary report pdf file.
        /// </summary>
        public string Report_Summary_Pdf { get; set; }

        /// <summary>
        /// Url to a full report pdf file.
        /// </summary>
        public string Report_Full_Pdf { get; set; }

    }
}
