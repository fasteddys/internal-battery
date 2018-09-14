using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PromoType
    {
        public int PromoTypeId { get; set; }
        public Guid? PromoTypeGuid { get; set; }
        [Required]
        public string PromoTypeName { get; set; }
        public string PromoTypeDescription { get; set; }
    }
}
