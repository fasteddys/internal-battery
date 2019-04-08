using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PartnerContactFileLeadStatus : BaseModel
    {
            public int PartnerContactFileId { get; set; }
            public virtual PartnerContactFile PartnerContactFile { get; set; }
            public int LeadStatusId { get; set; }
            public virtual LeadStatus LeadStatus { get; set; }
            public Guid? PartnerContactFileLeadStatusGuid { get; set; }
        
    }
}
