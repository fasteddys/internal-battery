using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ProfileViewDto
    {
  
        public Guid? SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }  
        public string PhoneNumber { get; set; }
        public string Address { get; set; } 
        public string City { get; set; }
        public string StateCode { get; set; }
        public string PostalCode { get; set; }  
        public List<SkillDto> Skills { get; set; }
        public List<SubscriberWorkHistoryDto> WorkHistory { get; set; }
        public List<SubscriberEducationHistoryDto> EducationHistory { get; set; } 
    }
}
