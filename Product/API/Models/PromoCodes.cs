using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PromoCodes
    {
        public int PromoCodesId { get; set; }
        public Guid? PromoCodesGuid { get; set; }
        [Required]
        public string PromoCode { get; set; }
        public DateTime PromoStartDate { get; set; }
        public DateTime PromoEndDate { get; set; }
        public int PromoTypeId { get; set; }
        public Decimal PromoValueFacotr { get; set; }
        [Required]
        public string PromoName { get; set; }
        public string PromoDescription { get; set; }
    }
}
