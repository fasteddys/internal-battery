using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PromoCodeRedemption : BaseModel
    {
        public int PromoCodeRedemptionId { get; set; }
        public Guid PromoCodeRedemptionGuid { get; set; }
        public DateTime? RedemptionDate { get; set; }
        public Decimal ValueRedeemed { get; set; }
        public string RedemptionNotes { get; set; }
        [Required]
        public int RedemptionStatusId { get; set; }
        public virtual RedemptionStatus RedemptionStatus { get; set; }
        public int PromoCodeId { get; set; }
        public virtual PromoCode PromoCode { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int CourseVariantId { get; set; }
        public virtual CourseVariant CourseVariant { get; set; }
    }
}