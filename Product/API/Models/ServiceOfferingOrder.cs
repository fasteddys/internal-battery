using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ServiceOfferingOrder : BaseModel
    {
        public int ServiceOfferingOrderId { get; set; }
        public Guid ServiceOfferingOrderGuid { get; set; }
        public int ServiceOfferingId { get; set; }
        public virtual ServiceOffering ServiceOffering { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal PricePaid { get; set; }
        public int PercentCommplete { get; set; }

        public int? PromoCodeId { get; set; }
        public virtual PromoCode PromoCode { get; set; }

        public string AuthorizationInfo { get; set; }
    }
}
