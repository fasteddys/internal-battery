using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobApplicationRecruiterViewDto : BaseDto
    {
        public Guid JobApplicationGuid { get; set; }

        public SubscriberDto Subscriber { get; set; }

        public string CoverLetter { get; set; }

        public string JobSeekerUrl { get; set; }
    }
}
