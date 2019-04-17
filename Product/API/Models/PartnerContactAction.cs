using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PartnerContactAction : BaseModel
    {
        public Guid? PartnerContactActionGuid { get; set; }

        public int PartnerContactId { get; set; }
        public virtual PartnerContact PartnerContact { get; set; }
        public int ActionId { get; set; }
        public virtual Action Action { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public DateTime OccurredDate { get; set; }
        public string Headers { get; set; }
        public int CampaignPhaseId { get; set; }
        public virtual CampaignPhase CampaignPhase { get; set; }
    }
}
