using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    [Obsolete("This is being replaced by CampaignPartnerContact", false)]
    public class CampaignContact : BaseModel
    {
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public int ContactId { get; set; }
        public virtual Contact Contact { get; set; }
        public Guid? CampaignContactGuid { get; set; }
    }
}
