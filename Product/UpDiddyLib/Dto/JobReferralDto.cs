using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobReferralDto
    {
        public string JobReferralGuid { get; set; }
        public string JobPostingGuidStr { get; set; }
        public string ReferrerGuid { get; set; }
        public string RefereeName { get; set; }
        public string RefereeEmailId { get; set; }
        public string RefereeGuid { get; set; }
        public string DescriptionEmailBody { get; set; }
        public string ReferUrl { get; set; }
        public Guid JobGuid { get; set; }
    }
}
