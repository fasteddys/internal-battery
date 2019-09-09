using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ServiceOfferingOrderDto : BaseDto
    {
        public Guid ServiceOfferingOrderGuid { get; set; }
        public ServiceOfferingDto ServiceOffering { get; set; }

        public  SubscriberDto Subscriber { get; set; }
        
        public Decimal PricePaid { get; set; }
        public int PercentCommplete { get; set; }

        public PromoCodeDto PromoCode { get; set; }

        public string AuthorizationInfo { get; set; }
    }
}
