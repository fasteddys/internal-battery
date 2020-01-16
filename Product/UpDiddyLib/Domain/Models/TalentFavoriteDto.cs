using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class TalentFavoriteListDto
    {
        public List<TalentFavoriteDto> TalentFavorites { get; set; } = new List<TalentFavoriteDto>();
        public int TotalRecords { get; set; }
    }

    public class TalentFavoriteDto
    {
        public Guid SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; } 
        public string PhoneNumber { get; set; } 
        public string ProfileImage { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime ModifyDate { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
