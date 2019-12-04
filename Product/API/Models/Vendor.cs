using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class Vendor : BaseModel
    {
        public int VendorId { get; set; }
        public Guid? VendorGuid { get; set; }
        [Required]
        public string Name { get; set; }

        public string LoginUrl { get; set; }
        public string LogoUrl { get; set; }
    }
}