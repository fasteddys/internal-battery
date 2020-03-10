using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models.G2
{
    public class WishlistListDto
    {
        public List<WishlistDto> Wishlists { get; set; } = new List<WishlistDto>();
        public int TotalRecords { get; set; }
    }

    public class WishlistDto
    {
        public Guid WishlistGuid { get; set; }
        public Guid RecruiterGuid { get; set; }
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
