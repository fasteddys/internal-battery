using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class PromoCodeDto
    {
        public bool IsValid { get; set; }
        public string PromoName { get; set; }
        public string PromoDescription { get; set; }
        public string ValidationMessage { get; set; }
        public Decimal Discount { get; set; }
        public Decimal FinalCost { get; set; }
        public Guid PromoCodeRedemptionGuid { get; set; }
    }
}
