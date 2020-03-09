using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UpDiddyLib.Domain.Models.G2
{
   public  class WishlistDto
    {
        public Guid WishlistGuid { get; set; }
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
    }
}
