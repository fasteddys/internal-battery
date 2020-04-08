using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("Wishlists", Schema = "G2")]
    public class Wishlist : BaseModel
    {
        public int WishlistId { get; set; }
        public Guid WishlistGuid { get; set; }
        public int RecruiterId { get; set; }
        public virtual Recruiter Recruiter { get; set; }
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
    }
}
