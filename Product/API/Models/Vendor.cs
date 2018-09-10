using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class Vendor
    {
        public int VendorId { get; set; } 
  
        [Required]
        public string Name { get; set; }

        public int IsDeleted { get; set; }
    }
}