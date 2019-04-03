using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PartnerContactLeadStatus : BaseModel
    {
        public int PartnerContactId { get; set; }
        public virtual PartnerContact PartnerContact { get; set; }
        public int LeadStatusId { get; set; }
        public virtual LeadStatus LeadStatus { get; set; }
        public Guid? PartnerContactLeadStatusGuid { get; set; }
    }
}