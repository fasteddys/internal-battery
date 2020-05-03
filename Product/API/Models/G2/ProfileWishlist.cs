using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ProfileWishlists", Schema = "G2")]
    public class ProfileWishlist : BaseModel
    {
        public int ProfileWishlistId { get; set; }
        public Guid ProfileWishlistGuid { get; set; }
        public int WishlistId { get; set; }
        public virtual Wishlist Wishlist { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }
    }
}