using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    [Obsolete("This is being replaced by PartnerContactAction", false)]
    public class ContactAction : BaseModel
    {
        public Guid? ContactActionGuid { get; set; }
        public int ContactId { get; set; }
        public virtual Contact Contact { get; set; }
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
