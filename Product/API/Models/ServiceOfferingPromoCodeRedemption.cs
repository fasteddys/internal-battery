using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ServiceOfferingPromoCodeRedemption : BaseModel
    {
        public int ServiceOfferingPromoCodeRedemptionId { get; set; }
        public Guid ServiceOfferingPromoCodeRedemptionGuid { get; set; }
        public DateTime? RedemptionDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal ValueRedeemed { get; set; }
        public string RedemptionNotes { get; set; }
        [Required]
        public int RedemptionStatusId { get; set; }
        public virtual RedemptionStatus RedemptionStatus { get; set; }
        public int PromoCodeId { get; set; }
        public virtual PromoCode PromoCode { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int ServiceOfferingId { get; set; }
        public virtual ServiceOffering ServiceOffering { get; set; }
    }
}
