using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CampaignPartner : BaseModel
    {
        public int CampaignPartnerId { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public int PartnerId { get; set; }
        public virtual Partner Partner { get; set; }
        public Guid? CampaignPartnerGuid { get; set; }
        public int? EmailDeliveryCap { get; set; }
        public int? EmailDeliveryLookbackInHours { get; set; }
        public bool IsUseSeedEmails { get; set; }
        public string EmailTemplateId { get; set; }
        public string EmailSubAccountId { get; set; }
        public int? UnsubscribeGroupId { get; set; }
    }
}
