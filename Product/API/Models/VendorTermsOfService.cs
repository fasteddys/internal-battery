using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class VendorTermsOfService : BaseModel
    {
        public int VendorTermsOfServiceId { get; set; }
        public Guid? VendorTermsOfServiceGuid { get; set; }
        public int VendorId { get; set; }
        public DateTime DateEffective { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
