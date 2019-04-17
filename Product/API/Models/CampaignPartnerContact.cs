using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CampaignPartnerContact : BaseModel
    {
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public int PartnerContactId { get; set; }
        public virtual PartnerContact PartnerContact { get; set; }
        public Guid? CampaignPartnerContactGuid { get; set; }
    }
}
