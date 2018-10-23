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
        public Guid PromoCodeGuid { get; set; }
        public Guid StudentGuid { get; set; }
        public Guid CourseGuid { get; set; }
        [Required]
        public DateTime RedemptionDate { get; set; }
        public Decimal ValueRedeemed { get; set; }
        public string RedemptionNotes { get; set; }
        [Required]
        public int RedemptionStatusId { get; set; }
        public virtual RedemptionStatus RedemptionStatus { get; set; }
    }
}
