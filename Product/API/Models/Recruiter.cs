using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Recruiter : BaseModel
    {
        public Guid RecruiterGuid { get; set; }
        public int RecruiterId { get; set; }
        public int? SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
