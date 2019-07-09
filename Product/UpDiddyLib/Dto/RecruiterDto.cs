using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class RecruiterDto
    {
        public Guid RecruiterGuid { get; set; } 
        public virtual SubscriberDto Subscriber { get; set; }
        public Guid SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string  PhoneNumber { get; set; }
        public virtual CompanyDto Company { get; set; }
        public bool isRecruiter { get; set; }

    }
}
