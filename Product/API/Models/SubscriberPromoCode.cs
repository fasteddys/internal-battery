using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberPromoCode : BaseModel
    {
        public int SubscriberPromoCodeId { get; set; }
        public Guid? SubscriberPromoCodeGuid { get; set; }
        public int SubscriberId { get; set; }
        public int PromoCodeId { get; set; }
        public int? MaxAllowedNumberOfRedemptions { get; set; }
        public int NumberOfRedemptions { get; set; }
    }
}
