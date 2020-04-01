using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models.G2
{
    public class ProfileWishlistListDto
    {
        public List<ProfileWishlistDto> WishlistProfiles { get; set; } = new List<ProfileWishlistDto>();
        public int TotalRecords { get; set; }
    }

    public class ProfileWishlistDto
    {
        public Guid ProfileWishlistGuid { get; set; }
        public Guid ProfileGuid { get; set; }
        public Guid RecruiterGuid { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
