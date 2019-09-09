using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ServiceOfferingPromoCode : BaseModel
    {
        public int ServiceOfferingPromoCodeId { get; set; }
        public Guid? ServiceOfferingPromoCodeGuid { get; set; }
        /// <summary>
        /// Specifies a specific service offering entry, -1 implies that the promo code is valid for 
        /// all service offerings.
        /// </summary>
        public int ServiceOfferingId { get; set; }
        public int PromoCodeId { get; set; }
        public int? MaxAllowedNumberOfRedemptions { get; set; }
        public int NumberOfRedemptions { get; set; }
    }
}
