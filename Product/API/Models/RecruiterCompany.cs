using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class RecruiterCompany : BaseModel
    {
        public int RecruiterCompanyId { get; set;  }
        public Guid RecruiterCompanyGuid { get; set; }
        public int RecruiterId { get; set; }
        public virtual Recruiter Recruiter { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company {get; set;}
 
    }
}
