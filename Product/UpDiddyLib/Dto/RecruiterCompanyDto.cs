using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class RecruiterCompanyDto : BaseDto
    {
        public int RecruiterCompanyId { get; set; }

        public Guid RecruiterCompanyGuid { get; set; }
 
        public SubscriberDto Subscriber { get; set; }
 
        public CompanyDto Company { get; set; }

        public Guid CompanyGuid { get; set; }
 
        public int IsStaff { get; set; }
    }
}
