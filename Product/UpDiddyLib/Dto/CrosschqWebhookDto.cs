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
    }
}
