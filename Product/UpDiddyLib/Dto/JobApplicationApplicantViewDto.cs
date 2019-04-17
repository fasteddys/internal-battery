using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{

    /// <summary>
    /// Job application without the posting 
    /// </summary>
    public class JobApplicationApplicantViewDto : BaseDto
    {
        public Guid JobApplicationGuid { get; set; }
      

        public JobPostingDto JobPosting { get; set; }

        public string CoverLetter { get; set; }

        public string JobPostingUrl { get; set; }

    }
}
