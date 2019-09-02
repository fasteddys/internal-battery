using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ServiceOfferingPromoCodeRedemptionDto : BaseDto
    {
        public Guid ServiceOfferingPromoCodeRedemptionGuid { get; set; }
        public DateTime? RedemptionDate { get; set; }     
        public Decimal ValueRedeemed { get; set; }
        public string RedemptionNotes { get; set; }
        public int RedemptionStatusId { get; set; }
        public  RedemptionStatusDto RedemptionStatus { get; set; }
        public PromoCodeDto PromoCode { get; set; }
        public  SubscriberDto Subscriber { get; set; }        
        public  ServiceOfferingDto ServiceOffering { get; set; }

    }
}
