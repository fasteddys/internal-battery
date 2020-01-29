using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{

    public class RecruiterInfoListDto
    {
        public List<RecruiterInfoDto> Entities { get; set; }
        public int TotalRecords { get; set; }
    }

    public class RecruiterInfoDto
    {

        [JsonIgnore]
        public int TotalRecords { get; set; }
        public Guid RecruiterGuid { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public Guid? CompanyGuid { get; set; }

        public string CompanyName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }        
        public bool? IsInAuth0RecruiterGroup { get; set; }
    }
}
