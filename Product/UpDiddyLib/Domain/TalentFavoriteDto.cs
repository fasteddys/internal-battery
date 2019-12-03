using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain
{
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
    }
}
