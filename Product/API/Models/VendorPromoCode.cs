using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class VendorPromoCode
    {
        public int VendorPromoCodeId { get; set; }
        public Guid? VendorPromoCodeGuid { get; set; }
        public int PromoCodeId { get; set; }
        public int VendorId { get; set; }
    }
}
