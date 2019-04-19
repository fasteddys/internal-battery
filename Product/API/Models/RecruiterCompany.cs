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

        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }

        public int CompanyId { get; set; }

        public virtual Company Company {get; set;}
        /// <summary>
        /// Set to 1 is the recruiter is employed by the company
        /// </summary>
        public int IsStaff { get; set;  }
    
    }
}
