using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PromoCode : BaseModel
    {
        public int PromoCodeId { get; set; }
        public Guid? PromoCodeGuid { get; set; }
        [Required]
        public string Code { get; set; }
        public DateTime PromoStartDate { get; set; }
        public DateTime PromoEndDate { get; set; }
        public int PromoTypeId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal PromoValueFactor { get; set; }
        [Required]
        public string PromoName { get; set; }
        public string PromoDescription { get; set; }
        public int NumberOfRedemptions { get; set; }
        public int MaxAllowedNumberOfRedemptions { get; set; }
        public virtual PromoType PromoType { get; set; }
        public int? MaxNumberOfRedemptionsPerSubscriber { get; set; }
    }
}
