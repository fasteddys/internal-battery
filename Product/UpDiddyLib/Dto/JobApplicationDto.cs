using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace UpDiddyLib.Dto
{
    public class JobApplicationDto : BaseDto
    { 
        public Guid JobApplicationGuid { get; set; }
 
        public JobPostingDto JobPosting { get; set; }
        
        public SubscriberDto Subscriber { get; set; }
    
        public string CoverLetter { get; set; }

        public PartnerDto Partner { get; set; }



    }
}
